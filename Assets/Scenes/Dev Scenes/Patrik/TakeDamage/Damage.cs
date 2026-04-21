using FishNet.Component.Prediction;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
     //TODO Have blackboard in use for controlling health 
     public class Damage : NetworkBehaviour
     {
          [SerializeField] private HealthSO healthData;
          [SerializeField] private NetworkTrigger networkTrigger;
          private readonly SyncVar<int> _healthCounter = new SyncVar<int>(10);
          
          public override void OnStartServer()
          {
               networkTrigger.OnEnter += TriggerDamage;
               healthData.UpdateHealth(new HealthPackage(_healthCounter.Value, 2));
          }

          public override void OnStartClient()
          {
               if (IsServerInitialized)
               {
                    Debug.Log("Skipped server");
               }
               base.OnStartClient();
               _healthCounter.OnChange += UpdateUI;
          }

          [Server]
          protected virtual void TriggerDamage(Collider collider)
          {
               if (collider.TryGetComponent(out IEffectData t))
               {
                    Debug.Log($"Hit: {_healthCounter}");
                    _healthCounter.Value--;
               }
          }

          protected virtual void UpdateUI(int prev, int next, bool asServer)
          {
               if (asServer) return;
               Debug.Log("Only clients");
               
               healthData.UpdateHealth(new HealthPackage(_healthCounter.Value, 2));
          }
     }
}
