using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachineScripts.Conditions
{
    public class NotInRangeCondition : InRangeCondition
    { 
        public NotInRangeCondition(Transform sightPoint, ITargetProvider targetProvider, float range) : base(sightPoint,targetProvider,range) { }
        
        public override bool Evaluate()
        {
            return !TargetsInRange(SightPoint, TargetProvider, Range);
        }
    }
}