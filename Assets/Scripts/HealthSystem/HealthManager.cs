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
          
          private uint HandleClientHealthChange(int clientID, int healthChange)
          {
               HealthPackage clientHealthPackage = _clientsHealth[clientID];
               int potentialNewHealth = (int)clientHealthPackage.HealthAmount + healthChange;
               uint maxHealth = clientHealthPackage.BatchAmount * healthRuleData.HealthPerBatch;
               
               if (potentialNewHealth < 0)
               {
                    return 0;
               }
               
               if (potentialNewHealth > maxHealth)
               {
                    return maxHealth;
               }
               
               return (uint)potentialNewHealth;
          }
          
          private bool HandleClientBatchChange(int clientID, int batchChange, out uint resultBatchAmount)
          {
               int possibleBatchAmount = (int)_clientsHealth[clientID].BatchAmount + batchChange;
               uint lowestAmount = 1;
               uint maxAmount = _currentMaxBatchAmountPerPlayer.Value;
               
               if (possibleBatchAmount < lowestAmount)
               {
                    resultBatchAmount = lowestAmount;
                    return false;
               }
               
               if (possibleBatchAmount > maxAmount)
               {
                    resultBatchAmount = maxAmount;
                    return false;
               }
               
               resultBatchAmount = (uint)possibleBatchAmount;
               return true;
          }
          
          [ServerRpc(RequireOwnership = false)]
          private void StoreHealthChanges(int clientID, HealthPackage package)
          {
               //uint currentBatchAmount =
               
               uint maxHealth = package.BatchAmount * healthRuleData.HealthPerBatch;
               uint currentHealth = ValidateValue(package.HealthAmount,0,maxHealth);
               
               _clientsHealth[clientID] = package;
          }

          private uint ValidateValue(uint current, uint minimum, uint maximum)
          {
               if (current < minimum)
               {
                    return minimum;
               }
               
               if (current > maximum)
               {
                    return maximum;
               }
               
               return current;
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
                    HealthPackage healthPackage = new HealthPackage()
                    {
                         HealthAmount = setHealth,
                         BatchAmount = setBatch
                    };
                    StoreHealthChanges(setOnClient, healthPackage);
                    simulateSet = false;
               }
               if (simulateChange)
               {
                    HealthPackage healthPackage = _clientsHealth[simulateClient];
                    
                    healthPackage.HealthAmount = HandleClientHealthChange(simulateClient,simulateHealthValues);
                    if (HandleClientBatchChange(simulateClient,simulateBatchValues, out uint amount))
                    {
                         healthPackage.BatchAmount = amount;
                    }
                    
                    StoreHealthChanges(simulateClient, healthPackage);
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
