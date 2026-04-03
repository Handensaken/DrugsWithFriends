using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.Conditions;
using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachineScripts.Conditions
{
    public class InSightCondition : BasicConditionCreation, ITransitionCondition
    {
        private readonly Transform sightPoint;
        private readonly ITargetProvider targetProvider;
        private readonly ISightData sightData;

        public InSightCondition(Transform sightPoint, ITargetProvider targetProvider, ISightData sightData)
        {
            this.sightPoint = sightPoint;
            this.targetProvider = targetProvider;
            this.sightData = sightData;
        }
        
        public bool Evaluate()
        {
            return CheckAnyTargetInSight(sightPoint, targetProvider, sightData);
        }

        private bool CheckAnyTargetInSight(Transform sightPoint, ITargetProvider targetProvider, ISightData sightData)
        {
            IEnemyTarget[] result = TargetsInRange(sightPoint, targetProvider.GetAllTargets, sightData.SightRange);
            if (result.Length <= 0) return false;
            
            result = TargetsInSightAngle(sightPoint, result, sightData.SightAngle);
            if (result.Length <= 0) return false;
            
            result = TargetsInRayCast(sightPoint, result, sightData.SightRange);

            return result.Length > 0;
        }
    }
}