using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachine.Solid.Scripts.Conditions
{
    public class SubsetInRangeCondition : BasicConditionCreation, ITransitionCondition
    {
        protected readonly Transform sightPoint;
        protected readonly ICertifiedTargetProvider certifiedTargetProvider;
        protected readonly float range;
        
        public SubsetInRangeCondition(Transform sightPoint, ICertifiedTargetProvider certifiedTargetProvider, float range)
        {
            this.sightPoint = sightPoint;
            this.certifiedTargetProvider = certifiedTargetProvider;
            this.range = range;
        }
        
        public virtual bool Evaluate()
        {
            return InRange(sightPoint, certifiedTargetProvider, range);
        }

        protected bool InRange(Transform sightPoint, ICertifiedTargetProvider certifiedTargetProvider, float range)
        {
            IEnemyTarget[] result = TargetsInRange(sightPoint, certifiedTargetProvider.GetCertainTargets(),range);
            return result.Length > 0;
        }
    }
}