using FishNet.Object;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using Unity.Behavior;
using UnityEngine;

namespace HealthSystem.OtherHealth
{
    public class NpcHealth : ObjectHealth
    {
        [Space,Header("NPC")]
        [SerializeField] private BehaviorGraphAgent behaviorAgent;
        [SerializeField, Min(1)] private uint batchAmount;
        public override void OnStartClient()
        {
            base.OnStartClient();
            healthBarUI.SetUpHealthBarNpc();
        }

        [Server]
        protected override void HandleDamage()
        {
            int currentHealth = (int)_healthCounter.Value;
            if (currentHealth-1 <= 0)
            {
                _healthCounter.Value = 0;
            }
            else
            {
                _healthCounter.Value--;
            }
            
            behaviorAgent.SetVariableValue("Health", (int)_healthCounter.Value);
        }
        
        [Server]
        protected override void TriggerDamage(Collider collider)
        {
            if (!collider.CompareTag("Enemy") && collider.TryGetComponent(out IEffectData t))
            {
                HandleDamage();
                behaviorAgent.SetVariableValue("Stagger", true);
            }
        }
        
        [Client]
        protected override void UpdateUI(uint prev, uint next, bool asServer)
        {
            if (asServer)
            {
                return;
            }
               
            //Debug.Log("Prev: "+prev +" - UpdateUI hp: "+next);
               
            HealthPackage hp = new HealthPackage()
            {
                HealthAmount = next,
                BatchAmount = batchAmount
            };
            healthBarUI.UpdateUI(hp);
        }
    }
}