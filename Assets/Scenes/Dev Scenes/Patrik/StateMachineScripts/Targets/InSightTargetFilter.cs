using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.Conditions;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachine.Solid.Scripts.Targets
{
    public class InSightTargetFilter : BasicConditionCreation, ITargetFilter
    {
        private readonly Transform sightTransform;
        private readonly ISightData sightData;

        public InSightTargetFilter(Transform sightTransform, ISightData sightData)
        {
            this.sightTransform = sightTransform;
            this.sightData = sightData;
        }
        
        private bool InSightFilter(IEnemyTarget enemyTarget)
        {
            if (!TargetInRange(sightTransform, enemyTarget, sightData.SightRange)) return false;
            if (!TargetInSightAngle(sightTransform, enemyTarget, sightData.SightAngle)) return false;
            return TargetInRayCast(sightTransform, enemyTarget, sightData.SightAngle);
        }

        public bool Filter(IEnemyTarget enemyTarget)
        {
            return InSightFilter(enemyTarget);
        }
    }
}