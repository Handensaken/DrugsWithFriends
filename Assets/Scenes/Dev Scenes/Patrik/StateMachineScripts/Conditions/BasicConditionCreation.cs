using System;
using System.Collections.Generic;
using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Targets;
using UnityEngine;

namespace StateMachine.Solid.Scripts.Conditions
{
    public abstract class BasicConditionCreation
    {
        protected IEnemyTarget[] TargetsInSightAngle(Transform eyes, IEnemyTarget[] targets, float angleOneSide)
        {
            Vector3 eyePos = new Vector3(eyes.position.x, 0, eyes.position.z);
            Vector3 eyeForward = new Vector3(eyes.forward.x, 0, eyes.forward.z).normalized;
            
            List<IEnemyTarget> result = new List<IEnemyTarget>();
            foreach (IEnemyTarget target in targets)
            {
                Vector3 targetPos = target.Position;
                Vector3 directionToTarget = (new Vector3(targetPos.x,0,targetPos.z) - eyePos)
                    .normalized;
                float angle = Vector3.Angle(eyeForward, directionToTarget);

                if (angle <= angleOneSide) result.Add(target);
            }
            
            return result.ToArray();
        }
        
        protected bool TargetInSightAngle(Transform eyes, IEnemyTarget enemyTarget, float angleOneSide)
        {
            Vector3 eyePos = new Vector3(eyes.position.x, 0, eyes.position.z);
            Vector3 eyeForward = new Vector3(eyes.forward.x, 0, eyes.forward.z).normalized;
            
            Vector3 targetPos = enemyTarget.Position;
            Vector3 directionToTarget = (new Vector3(targetPos.x,0,targetPos.z) - eyePos).normalized;
            float angle = Vector3.Angle(eyeForward, directionToTarget);
            
            return angle <= angleOneSide;
        }
        
        protected IEnemyTarget[] TargetsInRange(Transform sightPoint, IEnemyTarget[] targets ,float sightRange)
        {
            List<IEnemyTarget> result = new List<IEnemyTarget>();
            foreach (IEnemyTarget target in targets)
            {
                float distance = Vector3.Distance(sightPoint.position, target.Position);
                if(distance <= sightRange) result.Add(target);
            }

            return result.ToArray();
        }
        
        protected bool TargetInRange(Transform sightPoint, IEnemyTarget enemyTarget ,float sightRange)
        {
            float distance = Vector3.Distance(sightPoint.position, enemyTarget.Position);
            return distance <= sightRange;
        }

        protected IEnemyTarget[] TargetsInRayCast(Transform sightPoint, IEnemyTarget[] targets, float sightRange)
        {
            LayerMask layerMask = ~LayerMask.GetMask("Ignore Raycast");
            
            List<IEnemyTarget> result = new List<IEnemyTarget>();
            foreach (IEnemyTarget target in targets)
            {
                Vector3 dirToTarget = (target.Position - sightPoint.position).normalized;
                Physics.Raycast(sightPoint.position, dirToTarget, out RaycastHit hit, sightRange, layerMask);
                
                if(hit.collider == target.GetCollider) result.Add(target);
            }
            
            return result.ToArray();
        }
        
        protected bool TargetInRayCast(Transform sightPoint, IEnemyTarget enemyTarget, float sightRange)
        {
            LayerMask layerMask = ~LayerMask.GetMask("Ignore Raycast");
            
            Vector3 dirToTarget = (enemyTarget.Position - sightPoint.position).normalized;
            Physics.Raycast(sightPoint.position, dirToTarget, out RaycastHit hit, sightRange, layerMask);

            return hit.collider == enemyTarget.GetCollider;
        }
    }
}