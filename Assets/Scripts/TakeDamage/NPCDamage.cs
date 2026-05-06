using FishNet.Object;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TakeDamage
{
    public class NPCDamage : Damage
    {
        [SerializeField] private BehaviorGraphAgent behaviorAgent;

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