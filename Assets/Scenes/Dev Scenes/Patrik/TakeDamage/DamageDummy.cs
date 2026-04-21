using FishNet.Component.Prediction;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
     //TODO Have blackboard in use for controlling health 
     [RequireComponent(typeof(Rigidbody))]
     public class DamageDummy : Damage
     {
          private Rigidbody rb;
          public override void OnStartServer()
          {
               base.OnStartServer();
               if (TryGetComponent(out Rigidbody rigidbody))
               {
                    rb = rigidbody;
               }
               else
               {
                    Debug.LogError("Couldn't get rigidbody");
               }
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
          }

          [Server]
          protected override void TriggerDamage(Collider collider)
          {
               if (collider.TryGetComponent(out Weapon w))
               {
                    Debug.Log("dummy got hit");
               }
          }

          protected override void UpdateUI(int prev, int next, bool asServer)
          {
               base.UpdateUI(prev, next, asServer);
          }
     }
}
