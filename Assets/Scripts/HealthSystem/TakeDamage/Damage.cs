using FishNet.Component.Prediction;
using FishNet.Object;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
     /// <summary>
     /// Trigger logic for damage
     /// </summary>
     public class Damage : NetworkBehaviour
     {
          [Header("Damageable")] 
          [SerializeField] protected NetworkTrigger networkTrigger;
          
          public override void OnStartServer()
          {
               base.OnStartServer();
               networkTrigger.OnEnter += TriggerDamage;
          }

          public override void OnStopServer()
          {
               base.OnStopServer();
               networkTrigger.OnEnter -= TriggerDamage;
          }
          
          [Server]
          protected virtual void TriggerDamage(Collider collider)
          {
               if (collider.TryGetComponent(out IEffectData t))
               {
                    HandleDamage();
               }
          }
          
          [Server]
          protected virtual void HandleDamage()
          {
               Debug.Log($"Hit: {gameObject.name}");
          }
     }
}
