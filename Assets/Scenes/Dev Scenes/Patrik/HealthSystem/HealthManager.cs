using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scenes.Dev_Scenes.Patrik.Health_system;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
     public class HealthManager : NetworkBehaviour //TODO make total control over clients health
     {
          [SerializeField, Range(0,250)]private int simulateHealthChange0;
          [SerializeField, Range(0,250)]private int simulateHealthChange1;
          
          [SerializeField] private HealthSO healthSo;
          [SerializeField] private bool simulateChange;

          private readonly SyncDictionary<int, int> _clientsHealth = new SyncDictionary<int, int>(){{0,100},{1,50}};
          
          public void OnEnable()
          {
               _clientsHealth.OnChange += SetValues;
          }

          public void OnDisable()
          {
               _clientsHealth.OnChange -= SetValues;
          }

          private void SetValues(SyncDictionaryOperation op, int key, int value, bool asServer)
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
               _clientsHealth[0] = simulateHealthChange0;
               _clientsHealth[1] = simulateHealthChange1;
               Debug.Log(_clientsHealth);
          }
          
          [TargetRpc]
          private void SendHealth(NetworkConnection conn, int health)
          {
               Debug.Log("ClientID: "+conn.ClientId + " - " + health);
               HealthPackage currentHealthData = new HealthPackage(health,health);
               healthSo.UpdateHealth(currentHealthData);
          }
          
          
          
     }
}
