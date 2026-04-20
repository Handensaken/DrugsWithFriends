using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
     public class HealthManager : NetworkBehaviour //TODO make total control over clients health
     {
          [SerializeField]private int simulateHealthChange0;
          [SerializeField]private int simulateBatchChange0;
          [SerializeField]private int simulateHealthChange1;
          [SerializeField]private int simulateBatchChange1;
          
          [SerializeField] private HealthSO healthSo;
          [SerializeField] private bool simulateChange;

          private readonly SyncDictionary<int, HealthPackage> _clientsHealth = new SyncDictionary<int, HealthPackage>()
          {
               {0,new HealthPackage(10,2)},
               {1,new HealthPackage(5,1)}
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
               Debug.Log("Now");
               foreach (var client in ServerManager.Clients)
               {
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
               HealthPackage healthPackageC1 = new HealthPackage(simulateHealthChange0, simulateBatchChange0);
               _clientsHealth[0] = healthPackageC1;
               HealthPackage healthPackageC2 = new HealthPackage(simulateHealthChange1, simulateBatchChange1);
               _clientsHealth[1] = healthPackageC2;
               
               Debug.Log(_clientsHealth);
          }
          
          [TargetRpc]
          private void SendHealth(NetworkConnection conn, HealthPackage healthPackage)
          {
               int health = healthPackage.HealthAmount;
               int batch = healthPackage.BatchAmount;
               Debug.Log("ClientID: "+conn.ClientId + " - H: " + health + " - B: " + batch);
               HealthPackage currentHealthData = new HealthPackage(health,batch);
               healthSo.UpdateHealth(currentHealthData);
          }
     }
}
