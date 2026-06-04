using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using Scenes.Dev_Scenes.Patrik.TakeDamage;
using UnityEngine;

namespace HealthSystem.OtherHealth
{
     public class ObjectHealth : Damage
     {
          [Header("Object")]
          [SerializeField,Min(1)] protected uint initialHealth;
          protected readonly SyncVar<uint> _healthCounter = new SyncVar<uint>();
          
          [Space,SerializeField] protected HealthRuleData healthRuleData;
          [SerializeField] protected HealthBarUI healthBarUI;

          public override void OnStartServer()
          {
               base.OnStartServer();
               _healthCounter.Value = initialHealth;
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
               _healthCounter.OnChange += UpdateUI;
               
          }

          public override void OnStopClient()
          {
               base.OnStopClient();
               _healthCounter.OnChange -= UpdateUI;
          }

          [Server]
          protected override void TriggerDamage(Collider collider)
          {
               if (collider.TryGetComponent(out IEffectData t))
               {
                    HandleDamage();
               }
          }

          [Server]
          protected override void HandleDamage()
          {
               int currentHealth = (int)_healthCounter.Value;
               if (currentHealth-1 <= 0)
               {
                    _healthCounter.Value = 0;
                    HandleDestruction();
               }
               else
               {
                    _healthCounter.Value--;
               }
          }

          [ServerRpc(RequireOwnership = false)]
          public virtual void HandleDestruction()
          {
               NetworkObject.Despawn();
          }
          
          [Client]
          protected virtual void UpdateUI(uint prev, uint next, bool asServer)
          {
               if (asServer)
               {
                    return;
               }
               
               Debug.Log("Prev: "+prev +" - UpdateUI hp: "+next);
               
               HealthPackage hp = new HealthPackage()
               {
                    HealthAmount = next,
                    BatchAmount = 1
               };
               healthBarUI.UpdateUI(hp);
          }
     }
}
