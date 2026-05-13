using FishNet.Object;
using Unity.Behavior;
using UnityEngine;

namespace HealthSystem.OtherHealth
{
    public class NpcHealth : ObjectHealth
    {
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
    }
}