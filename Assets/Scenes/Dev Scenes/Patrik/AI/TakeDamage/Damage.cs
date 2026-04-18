using System;
using FishNet.Component.Prediction;
using FishNet.Object;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.TakeDamage
{
     public class Damage : NetworkBehaviour
     {
          [SerializeField] private NetworkTrigger networkTrigger;
          private int _hitCounter;
          
          public override void OnStartServer()
          {
               networkTrigger.OnEnter += TriggerDamage;
          }

          [Server]
          private void TriggerDamage(Collider collider)
          {
               if (collider.TryGetComponent<IWeapon>(out IWeapon t))
               {
                    Debug.Log($"Hit: {_hitCounter}");
                    _hitCounter++;
               }
          }
     }
}
