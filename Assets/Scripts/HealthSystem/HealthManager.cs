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
          [SerializeField]private int simulateHealthChange0;
          [SerializeField]private int simulateBatchChange0;
          [SerializeField]private int simulateHealthChange1;
          [SerializeField]private int simulateBatchChange1;
          
          [SerializeField] private HealthRuleData healthRuleData;
          [SerializeField] private bool simulateChange;

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
               //ServerManager.OnRemoteConnectionState += HandleClientChange;
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
               RequestHealth(ClientManager.Connection.ClientId);
          }

          public HealthPackage ReadHealth(int clientID)
          {
               return _clientsHealth[clientID];
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
               HealthPackage healthPackageC1 = new HealthPackage()
               {
                    HealthAmount = simulateHealthChange0,
                    BatchAmount = simulateBatchChange0
               };
               _clientsHealth[0] = healthPackageC1;
               
               HealthPackage healthPackageC2 = new HealthPackage()
               {
                    HealthAmount = simulateHealthChange1,
                    BatchAmount = simulateBatchChange1
               };
               _clientsHealth[1] = healthPackageC2;
          }
          
          [ObserversRpc]
          private void SendHealth(int index, HealthPackage healthPackage)
          {
               int health = healthPackage.HealthAmount;
               int batch = healthPackage.BatchAmount;
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
