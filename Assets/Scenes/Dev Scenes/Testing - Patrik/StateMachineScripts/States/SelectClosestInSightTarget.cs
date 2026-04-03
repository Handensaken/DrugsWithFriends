using Paket.StateMachineScripts.Targets;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachineScripts.States
{
    public class SelectClosestInSightTarget : ISelectTarget
    {
        private readonly ICertifiedTargetProvider targetProvider;
        private readonly Transform sightTransform;
        
        public SelectClosestInSightTarget(ICertifiedTargetProvider targetProvider, Transform sightTransform)
        {
            this.targetProvider = targetProvider;
            this.sightTransform = sightTransform;

        }
        
        public bool Select(out IEnemyTarget selectedEnemyTarget)
        {
            return FindClosestTarget(out selectedEnemyTarget, sightTransform, targetProvider);
        }

        private bool FindClosestTarget(out IEnemyTarget result, Transform sightTransform, ICertifiedTargetProvider targetProvider)
        {
            result = null;
            
            IEnemyTarget[] allTargets = targetProvider.GetCertainTargets();
            if (allTargets.Length <= 0) return false;

            foreach (IEnemyTarget target in allTargets)
            {
                if (result == null) result = target;
                if (Vector3.Distance(sightTransform.position,target.Position) < Vector3.Distance(sightTransform.position, result.Position))
                {
                    result = target;
                }
            }

            return true;
        }
        
    }
}