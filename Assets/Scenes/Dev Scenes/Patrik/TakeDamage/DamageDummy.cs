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
          [SerializeField] private float resetDelay = 2f;
          private Vector3 startPos;
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
               startPos = rb.position;
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
          }

          [Server]
          protected override void TriggerDamage(Collider collider)
          {
               Debug.Log("HIT");
               //base.TriggerDamage(collider);
               if (collider.TryGetComponent(out IEffectData effect))
               {
                    switch (effect)
                    {
                         case Sword sword:
                              sword.ApplyEffect(rb, collider);
                              StartCoroutine(ResetAfterDelay());
                              break;  
                    }
               }
          }

          [Server]
          System.Collections.IEnumerator ResetAfterDelay()
          {
               yield return new WaitForSeconds(resetDelay);
               
               rb.linearVelocity = Vector3.zero;
               rb.angularVelocity = Vector3.zero;
               transform.position = startPos;
          }

          protected override void UpdateUI(int prev, int next, bool asServer)
          {
               base.UpdateUI(prev, next, asServer);
          }
     }
}
