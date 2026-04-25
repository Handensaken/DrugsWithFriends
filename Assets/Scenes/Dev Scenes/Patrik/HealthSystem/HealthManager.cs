using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
          
          [FormerlySerializedAs("healthData")] [SerializeField] private HealthSO healthSo;
          [SerializeField] private bool simulateChange;

          private readonly SyncDictionary<int, HealthPackage> _clientsHealth = new SyncDictionary<int, HealthPackage>()
          {
               {0,new HealthPackage()
               {
                    HealthAmount = 10,
                    BatchAmount = 2
               }},
               //{1,new HealthPackage(5,1)}
          };

          public void OnEnable()
          {
               _clientsHealth.OnChange += SetValues;
          }

          public void OnDisable()
          {
               _clientsHealth.OnChange -= SetValues;
          }

          private void SetValues(SyncDictionaryOperation op, int key, HealthPackage value, bool asServer)
          {
               if (!asServer) return;
               foreach (var client in ServerManager.Clients)
               {
                    Debug.Log("SendHealth: "+ "ClientID: "+client.Value.ClientId +" - "+_clientsHealth[client.Value.ClientId].HealthAmount +" : "+ _clientsHealth[client.Value.ClientId].BatchAmount);
                    SendHealth(client.Value, _clientsHealth[client.Value.ClientId]);
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
          private void ChangeValues()
          {
               Debug.Log("ChangeValues");
               HealthPackage healthPackageC1 = new HealthPackage()
               {
                    HealthAmount = simulateHealthChange0,
                    BatchAmount = simulateBatchChange0
               };
               _clientsHealth[0] = healthPackageC1;
               /*HealthPackage healthPackageC2 = new HealthPackage(simulateHealthChange1, simulateBatchChange1);
               _clientsHealth[1] = healthPackageC2;*/
               
               Debug.Log(_clientsHealth[0].HealthAmount +" : "+ _clientsHealth[0].BatchAmount);
          }
          
          [TargetRpc]
          private void SendHealth(NetworkConnection conn, HealthPackage healthPackage)
          {
               int health = healthPackage.HealthAmount;
               int batch = healthPackage.BatchAmount;
               Debug.Log("ClientID: "+conn.ClientId + " - H: " + health + " - B: " + batch);
               HealthPackage currentHealthData = new HealthPackage()
               {
                    HealthAmount = health,
                    BatchAmount = batch
               };
               healthSo.UpdateHealth(currentHealthData);
          }
     }
}
