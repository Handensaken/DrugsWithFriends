using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachine.Solid.Scripts.Conditions
{
    public class SubsetNotInRangeCondition : SubsetInRangeCondition
    {
        public SubsetNotInRangeCondition(Transform sightPoint, ICertifiedTargetProvider certifiedTargetProvider, float range) : base(sightPoint, certifiedTargetProvider, range) {}
        
        public override bool Evaluate()
        {
            return !InRange(sightPoint, certifiedTargetProvider, range);
        }
    }
}