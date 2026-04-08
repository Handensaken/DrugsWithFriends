using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.Conditions;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachineScripts.Conditions
{
    public class NotInSightCondition : BasicConditionCreation, ITransitionCondition
    {
        private readonly Transform sightPoint;
        private readonly ITargetProvider targetProvider;
        private readonly ISightData sightData;

        public NotInSightCondition(Transform sightPoint, ITargetProvider targetProvider, ISightData sightData)
        {
            this.sightPoint = sightPoint;
            this.targetProvider = targetProvider;
            this.sightData = sightData;
        }
        
        public bool Evaluate()
        {
            return CheckNoTargetInSight(sightPoint, targetProvider, sightData);
        }
        
        private bool CheckNoTargetInSight(Transform sightPoint, ITargetProvider targetProvider,ISightData sightData)
        {
            IEnemyTarget[] result = targetProvider.GetAllTargets;
            if (result.Length <= 0) return true;
            
            result = TargetsInRange(sightPoint, result, sightData.SightRange);
            if (result.Length <= 0) return true;
            
            result = TargetsInSightAngle(sightPoint, result, sightData.SightAngle);
            if (result.Length <= 0) return true;

            result = TargetsInRayCast(sightPoint, result, sightData.SightRange);
            return result.Length <= 0;
        }
    }
}