using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
     public class HealthManager : NetworkBehaviour //TODO make total control over clients health
     {
          [Header("Set values to:")]
          [SerializeField]private int setOnClient;
          [SerializeField] private uint setHealth;
          [SerializeField] private uint setBatch;
          [SerializeField] private bool simulateSet;
          
          [Space,Header("Simulate healthChange:")]
          [SerializeField]private int simulateClient;
          [SerializeField] private int simulateHealthValues;
          [SerializeField] private int simulateBatchValues;
          [SerializeField] private bool simulateChange;
          
          [Space,SerializeField] private HealthRuleData healthRuleData;
          
          private readonly SyncVar<uint> _currentMaxBatchAmountPerPlayer = new SyncVar<uint>();
          private readonly SyncDictionary<int, HealthPackage> _clientsHealth = new SyncDictionary<int, HealthPackage>();

          private static HealthManager _singleton;
          public static HealthManager Instance => _singleton;

          #region Getters

          public uint MaxBatchAmount => _currentMaxBatchAmountPerPlayer.Value;
          public uint HealthPerBatch => healthRuleData.HealthPerBatch;
          
          public HealthPackage ReadClientHealth(int clientID)
          {
               if (!_clientsHealth.TryGetValue(clientID, out HealthPackage package))
               {
                    Debug.LogWarning("Missing client even though clientId-number is called");
                    return new HealthPackage();
               }
               
               return package;
          }

          #endregion
          
          #region SetUp
          
          private void Awake()
          {
               if (_singleton != null)
               {
                    Destroy(gameObject);
                    return; 
               }

               _singleton = this;
          }

          public void OnEnable()
          {
               _clientsHealth.OnChange += HandleSyncDictChange; // TODO put in StartServer
          }

          public void OnDisable()
          {
               _clientsHealth.OnChange -= HandleSyncDictChange;
          }
          
          #endregion
          
          public override void OnStartServer()
          {
               base.OnStartServer();
               ServerManager.OnRemoteConnectionState += RemoveClientHealth;
               
               _currentMaxBatchAmountPerPlayer.Value = healthRuleData.InitialMaxAmountForBatches;
               healthRuleData.RequestHealth += RequestHealth;
          }

          public override void OnStopServer()
          {
               base.OnStopServer();
               healthRuleData.RequestHealth -= RequestHealth;
               
               ServerManager.OnRemoteConnectionState -= RemoveClientHealth;
          }

          private void RemoveClientHealth(NetworkConnection networkConnection, RemoteConnectionStateArgs remoteConnectionStateArgs)
          {
               if (remoteConnectionStateArgs.ConnectionState != RemoteConnectionState.Stopped) return;
               
               int clientID = networkConnection.ClientId;
               _clientsHealth.Remove(clientID);
          }
          
          [ServerRpc(RequireOwnership = false)]
          private void StoreHealthChanges(int clientID, int health, int batchAmount)
          {
               uint currentBatchAmount = ValidateValue(batchAmount, 1, _currentMaxBatchAmountPerPlayer.Value);
               
               uint maxHealth = currentBatchAmount * healthRuleData.HealthPerBatch;
               uint currentHealth = ValidateValue(health,0,maxHealth);

               HealthPackage package = new HealthPackage()
               {
                    HealthAmount = currentHealth,
                    BatchAmount = currentBatchAmount
               };
               
               _clientsHealth[clientID] = package;
          }

          private uint ValidateValue(int current, uint minimum, uint maximum)
          {
               if (current < minimum)
               {
                    return minimum;
               }
               
               if (current > maximum)
               {
                    return maximum;
               }
               
               return (uint)current;
          }
          
          private void HandleSyncDictChange(SyncDictionaryOperation op, int key, HealthPackage value, bool asServer)
          {
               if (!asServer) return;

               if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set) //TODO dont know if the hole circle is necessary
               {
                    foreach (var keyValues in _clientsHealth)
                    {
                         SendUpdateHealth(keyValues.Key, keyValues.Value);
                    }
               }

               if (op == SyncDictionaryOperation.Remove)
               {
                    SendRemoveClient(key);
               }
          }
          //TODO remove - currently all clients
          private void Update()
          {
               if (simulateSet)
               {
                    StoreHealthChanges(setOnClient, (int)setHealth, (int)setBatch);
                    simulateSet = false;
               }
               
               //TODO handle if no change to batch-value (player gets the batch back and the other one never receives it)
               if (simulateChange)
               {
                    HealthPackage healthPackage = _clientsHealth[simulateClient];

                    int currentHealth = (int)healthPackage.HealthAmount + simulateHealthValues;
                    int currentBatchAmount = (int)healthPackage.BatchAmount + simulateBatchValues;
                    
                    StoreHealthChanges(simulateClient, currentHealth, currentBatchAmount);
                    simulateChange = false;
               }
          }

          [ServerRpc(RequireOwnership = false)]
          public void TryGiveBatchAmount(int givingClientID,int gettingClientID, uint change)
          {
               if (!_clientsHealth.TryGetValue(givingClientID, out HealthPackage givingHealthPackage) 
                   || !_clientsHealth.TryGetValue(gettingClientID, out HealthPackage gettingHealthPackage))
               {
                    Debug.LogWarning("Missing clients to give or to take batch from!");
                    return;
               }
               
               //check if values is valid
               int newGivingClientBatchAmount = (int)givingHealthPackage.BatchAmount - (int)change;
               if (newGivingClientBatchAmount <= 0)
               {
                    Debug.LogWarning("Giving client has to low batchAmount to give");
                    return;
               }
               int newGettingClientBatchAmount = (int)(gettingHealthPackage.BatchAmount + change);
               if (newGettingClientBatchAmount > _currentMaxBatchAmountPerPlayer.Value)
               {
                    return;
               }
               
               Debug.Log($"Giving: {givingClientID} : {newGivingClientBatchAmount} - Gets: {gettingClientID} : {newGettingClientBatchAmount}");
               
               //First we remove the batch
               StoreHealthChanges(givingClientID,(int)givingHealthPackage.HealthAmount,newGivingClientBatchAmount);
               
               //Then we give one batch to the chosen client
               StoreHealthChanges(gettingClientID, (int)gettingHealthPackage.HealthAmount, newGettingClientBatchAmount);
          }
          
          //TODO does it really need to be ServerRPC?
          [ServerRpc(RequireOwnership = false)]
          private void RequestHealth(int clientId)
          {
               //TODO testing values 4 now
               Debug.Log("RequestHealth - "+clientId);
               if (clientId == 0)
               {
                    HealthPackage healthPackage = new HealthPackage()
                    {
                         HealthAmount = 10,
                         BatchAmount = 3
                    };
                    _clientsHealth[clientId] = healthPackage;
                    return;
               }

               if (clientId == 1)
               {
                    HealthPackage healthPackage = new HealthPackage()
                    {
                         HealthAmount = 2,
                         BatchAmount = 1
                    };
                    _clientsHealth[clientId] = healthPackage;
                    return;
               }
               
               
               HealthPackage t = new HealthPackage()
               {
                    HealthAmount = 10,
                    BatchAmount = 1
               };
               _clientsHealth[clientId] = t;
          }
          
          [ObserversRpc]
          private void SendUpdateHealth(int index, HealthPackage healthPackage)
          {
               uint health = healthPackage.HealthAmount;
               uint batch = healthPackage.BatchAmount;
               HealthPackage currentHealthData = new HealthPackage()
               {
                    HealthAmount = health,
                    BatchAmount = batch
               };
               healthRuleData.UpdateHealth(index,currentHealthData);
          }
          
          [ObserversRpc]
          private void SendRemoveClient(int index)
          {
               healthRuleData.RemovalClientData(index);
          }
     }
}
