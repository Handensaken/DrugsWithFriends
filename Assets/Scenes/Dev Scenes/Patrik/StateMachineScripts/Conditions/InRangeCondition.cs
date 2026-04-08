using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.Scripts.Conditions;
using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachineScripts.Conditions
{
    public class InRangeCondition : BasicConditionCreation, ITransitionCondition
    {
        protected readonly Transform SightPoint;
        protected readonly ITargetProvider TargetProvider;
        protected readonly float Range;

        public InRangeCondition(Transform sightPoint, ITargetProvider targetProvider, float range)
        {
            SightPoint = sightPoint;
            TargetProvider = targetProvider;
            Range = range;
        }
        
        public virtual bool Evaluate()
        {
            return TargetsInRange(SightPoint, TargetProvider, Range);
        }
        
        protected bool TargetsInRange(Transform sightPoint, ITargetProvider targetProvider,float range)
        {
            IEnemyTarget[] result = TargetsInRange(sightPoint, targetProvider.GetAllTargets, range);
            return result.Length > 0;
        }
    }
}