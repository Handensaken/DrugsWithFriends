using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
    [Serializable]
    public class SightVisualization : IVisualization
    {
        //TODO flags
        [SerializeField] private Transform eyes;
        [SerializeField, Tooltip("How the visualization should occur")] private Visualization visualization;

        public Visualization GetVisualization => visualization;
        
        public void Visualize(Color gizmoColor, SightPackage sightPackage)
        {
            Gizmos.color = gizmoColor;
            DrawFOV(sightPackage.FOVRange, sightPackage.FOVAngle);
            SightRange(sightPackage.InstantInRange);
        }

        private void DrawFOV(float range, float angle)
        {
            SightRange(range);
            SightAngle(range,angle);
        }
        
        private void SightRange(float range)
        {
            Gizmos.DrawWireSphere(eyes.position,range);
        }
        
        private void SightAngle(float range, float angle) //Shame
        {
            //Only need x, z
            Vector3 worldPos = eyes.position;
            Vector3 forward = eyes.forward;

            float sightRange = range;
            float sightAngle = angle;

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