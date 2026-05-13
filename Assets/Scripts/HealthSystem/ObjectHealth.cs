using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using Scenes.Dev_Scenes.Patrik.TakeDamage;
using UnityEngine;

namespace HealthSystem
{
     public class ObjectHealth : Damage
     {
          [SerializeField,Min(1)] protected uint initialHealth;
          private readonly SyncVar<uint> _healthCounter = new SyncVar<uint>();
          
          [Space,SerializeField] protected HealthRuleData healthRuleData;
          [SerializeField] protected HealthBarUI healthBarUI;

          public override void OnStartServer()
          {
               base.OnStartServer();
               SetUpObject();
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
               _healthCounter.OnChange += UpdateUI;
               //UpdateUI();
          }

          public override void OnStopClient()
          {
               base.OnStopClient();
               _healthCounter.OnChange -= UpdateUI;
          }

          protected virtual void SetUpObject()
          {
               _healthCounter.Value = initialHealth;
          }
          
          [Client]
          protected virtual void UpdateUI(uint prev, uint next, bool asServer)
          {
               if (asServer)
               {
                    Debug.Log("Server here?");
               }
               
               HealthPackage hp = new HealthPackage()
               {
                    HealthAmount = next,
                    BatchAmount = 1
               };
               healthBarUI.UpdateUI(hp);
          }

          [Server]
          protected override void TriggerDamage(Collider collider)
          {
               base.TriggerDamage(collider);
               _healthCounter.Value--;
          }
          
          
     }
}
