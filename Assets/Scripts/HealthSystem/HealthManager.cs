using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

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

          public void OnEnable()
          {
               _clientsHealth.OnChange += SetValues;
          }

          public void OnDisable()
          {
               _clientsHealth.OnChange -= SetValues;
          }

          public override void OnStartServer()
          {
               base.OnStartServer();
               _currentMaxBatchAmountPerPlayer.Value = healthRuleData.InitialMaxAmountForBatches;
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
               RequestHealth(ClientManager.Connection.ClientId);
          }

          public uint MaxBatchAmount => _currentMaxBatchAmountPerPlayer.Value;
          
          public HealthPackage ReadClientHealth(int clientID)
          {
               return _clientsHealth[clientID];
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
          
          private void SetValues(SyncDictionaryOperation op, int key, HealthPackage value, bool asServer)
          {
               if (!asServer) return;

               if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
               {
                    foreach (var keyValues in _clientsHealth)
                    {
                         SendHealth(keyValues.Key, keyValues.Value);
                    }
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
          private void RequestHealth(int clientId)
          {
               HealthPackage healthPackage = new HealthPackage()
               {
                    HealthAmount = 10,
                    BatchAmount = 3
               };
               _clientsHealth[clientId] = healthPackage;
          }
          
          [ObserversRpc]
          private void SendHealth(int index, HealthPackage healthPackage)
          {
               uint health = healthPackage.HealthAmount;
               uint batch = healthPackage.BatchAmount;
               Debug.Log("ClientID: "+index + " - H: " + health + " - B: " + batch);
               HealthPackage currentHealthData = new HealthPackage()
               {
                    HealthAmount = health,
                    BatchAmount = batch
               };
               healthRuleData.UpdateHealth(index,currentHealthData);
          }
     }
}
