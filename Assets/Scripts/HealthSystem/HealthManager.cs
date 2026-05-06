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
          [SerializeField]private int simulateClient;
          [SerializeField] private uint simulateHealthValues;
          [SerializeField] private uint simulateBatchValues;
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

          public void HandleClientHealthChange(int clientID, int healthChange)
          {
               HealthPackage clientHealthPackage = _clientsHealth[clientID];
               int newHealth = (int)clientHealthPackage.HealthAmount + healthChange;
               uint maxHealth = clientHealthPackage.BatchAmount * healthRuleData.HealthPerBatch;
               
               if (newHealth < 0)
               {
                    clientHealthPackage.HealthAmount = 0;
               }
               else if (newHealth > maxHealth)
               {
                    clientHealthPackage.HealthAmount = maxHealth;
               }
               else
               {
                    clientHealthPackage.HealthAmount = (uint)newHealth;
               }
          }
          
          public bool HandleClientBatchChange(int clientID, int batchChange)
          {
               HealthPackage clientHealthPackage = _clientsHealth[clientID];
               int newBatchAmount = (int)clientHealthPackage.BatchAmount + batchChange;
               uint lowestAmount = 1;
               uint maxAmount = _currentMaxBatchAmountPerPlayer.Value;
               
               if (newBatchAmount < lowestAmount)
               {
                    clientHealthPackage.BatchAmount = lowestAmount;
                    return false;
               }
               
               if (newBatchAmount > maxAmount)
               {
                    clientHealthPackage.BatchAmount = maxAmount;
                    return false;
               }
               
               clientHealthPackage.HealthAmount = (uint)newBatchAmount;
               return true;
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
          private void Update()
          {
               if (simulateChange)
               {
                    ChangeValues();
                    simulateChange = false;
               }
          }

          [ServerRpc(RequireOwnership = false)]
          private void RequestHealth(int clientId)
          {
               HealthPackage healthPackage = new HealthPackage()
               {
                    HealthAmount = 10,
                    BatchAmount = 2
               };
               _clientsHealth[clientId] = healthPackage;
          }
          
          [ServerRpc(RequireOwnership = false)]
          private void ChangeValues()
          {
               Debug.Log("ChangeValues");
               HealthPackage newHealthPackage = new HealthPackage()
               {
                    HealthAmount = simulateHealthValues,
                    BatchAmount = simulateBatchValues
               };
                    
               _clientsHealth[simulateClient] = newHealthPackage;
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
