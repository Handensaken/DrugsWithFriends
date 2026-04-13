using System;
using UnityEngine;

namespace BehaviourTree
{
    public class VisualisationEnemyAI : VisualisationAI
    {
        [SerializeField] private Transform eyes;
        [SerializeField] protected EnemyData enemyData;
        
        public void OnDrawGizmos()
        {
            if (!settings.gizmosAlways) return;
            
            HandleGizmoColor();
            SightArea();
        }

        public void OnDrawGizmosSelected()
        {
            if (!settings.gizmosOnSelected) return;
            
            HandleGizmoColor();
            SightArea();
        }

        private void HandleGizmoColor()
        {
            Color color = enemyData.stateParameters.StateColor;
            color.a = 1;
            Gizmos.color = color;
        }
        
        private void SightArea()
        {
            SightRange();
            SightAngle();
        }

        private void SightRange()
        {
            Gizmos.DrawWireSphere(eyes.position,enemyData.stateParameters.sightParameters.Range);
        }
        
        private void SightAngle() //Shame
        {
            //Only need x, z
            Vector3 worldPos = eyes.position;
            Vector3 forward = eyes.forward;

            float sightRange = enemyData.stateParameters.sightParameters.Range;
            float sightAngle = enemyData.stateParameters.sightParameters.Angle;

            //LeftSide
            Vector2 valuesForLeftSide = RotateVectorCounter(new Vector2(forward.x,forward.z), sightAngle);
            Vector3 leftSide = new Vector3(valuesForLeftSide.x, 0, valuesForLeftSide.y)*sightRange;

            Vector3 leftCubePos = worldPos + leftSide;
            Gizmos.DrawLine(worldPos, leftCubePos);
            Gizmos.DrawCube(leftCubePos, new Vector3(.1f,.1f,.1f));
        
            
            //RightSide
            Vector2 valuesForRightSide = RotateVectorClock(new Vector2(forward.x,forward.z), sightAngle);
            Vector3 rightSide = new Vector3(valuesForRightSide.x, 0, valuesForRightSide.y)*sightRange;

            Vector3 rightCubePos = worldPos + rightSide;
            Gizmos.DrawLine(worldPos, rightCubePos);
            Gizmos.DrawCube(rightCubePos, new Vector3(.1f,.1f,.1f));
        }
        
        private Vector2 RotateVectorCounter(Vector2 inputVector, float angle)
        {
            if (angle <= 0) throw new ArgumentException("RotateVectorCounter can't and shouldn't handle angle less or equal to 0");
        
            float vectorX = inputVector.x * Mathf.Cos(Mathf.Deg2Rad * angle) +
                            inputVector.y * -Mathf.Sin(Mathf.Deg2Rad * angle);
            float vectorY = inputVector.x * Mathf.Sin(Mathf.Deg2Rad * angle) +
                            inputVector.y * Mathf.Cos(Mathf.Deg2Rad * angle);

            return new Vector2(vectorX, vectorY);
        }
        private Vector2 RotateVectorClock(Vector2 inputVector, float angle)
        {
            if (angle <= 0) throw new ArgumentException("RotateVectorCounter can't and shouldn't handle angle less or equal to 0");
        
            float vectorX = inputVector.x * Mathf.Cos(Mathf.Deg2Rad * angle) +
                            inputVector.y * Mathf.Sin(Mathf.Deg2Rad * angle);
            float vectorY = inputVector.x * -Mathf.Sin(Mathf.Deg2Rad * angle) +
                            inputVector.y * Mathf.Cos(Mathf.Deg2Rad * angle);

            return new Vector2(vectorX, vectorY);
        }
        
        
    }
}