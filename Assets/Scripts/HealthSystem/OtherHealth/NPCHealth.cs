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
            healthBarUI.SetUp4Npc();
        }
        
        [Server]
        protected override void TriggerDamage(Collider collider)
        {
            base.TriggerDamage(collider);
            if (collider.TryGetComponent(out IEffectData t))
            {
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
               
            Debug.Log("Prev: "+prev +" - UpdateUI hp: "+next);
               
            HealthPackage hp = new HealthPackage()
            {
                HealthAmount = next,
                BatchAmount = batchAmount
            };
            healthBarUI.UpdateUI(hp);
        }
    }
}