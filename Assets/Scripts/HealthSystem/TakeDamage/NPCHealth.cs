using FishNet.Object;
using HealthSystem;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using Unity.Behavior;
using UnityEngine;

namespace TakeDamage
{
    public class NpcHealth : ObjectHealth
    {
        [SerializeField] private BehaviorGraphAgent behaviorAgent;
        [SerializeField, Min(1)] private uint batchAmount;
          
        /*public override void OnStartClient()
        {
            base.OnStartClient();
            healthRuleData.UpdateHealth(ClientManager.Connection.ClientId,new HealthPackage()
            {
                HealthAmount = _healthCounter.Value,
                BatchAmount = 2
            });
        }*/
          
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