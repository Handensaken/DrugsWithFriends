using FishNet.Component.Prediction;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
     //TODO Have blackboard in use for controlling health 
     public class DamageDummy : Damage
     {
          [SerializeField] private HealthSO healthData;
          [SerializeField] private NetworkTrigger networkTrigger;
          private readonly SyncVar<int> _healthCounter = new SyncVar<int>(10);
          
          public override void OnStartServer()
          {
               base.OnStartServer();
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
          }

          [Server]
          protected void TriggerDamage(Collider collider)
          {
               base.TriggerDamage(collider);
          }

          private void UpdateUI(int prev, int next, bool asServer)
          {
               if (asServer) return;
               Debug.Log("Only clients");
               
               healthData.UpdateHealth(new HealthPackage(_healthCounter.Value, 2));
          }
     }
}
