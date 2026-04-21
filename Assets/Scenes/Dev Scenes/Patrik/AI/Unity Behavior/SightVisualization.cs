using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
    [Serializable]
    public class SightVisualization : IVisualization
    {
        [SerializeField] Transform eyes;
        [SerializeField] SightInfo sightInfo;
        [SerializeField] public bool onlySelectedGizmos;
    
        public void Visualize()
        {
            ValidateData();
            SightArea();
        }

        private void ValidateData()
        {
            if (sightInfo.angle <= 0) sightInfo.angle = 1;
            if (sightInfo.range <= 0) sightInfo.range = .1f;
        }
        
        private void SightArea()
        {
            SightRange();
            SightAngle();
        }

        private void SightRange()
        {
            Gizmos.DrawWireSphere(eyes.position,sightInfo.range);
        }
        
        private void SightAngle() //Shame
        {
            //Only need x, z
            Vector3 worldPos = eyes.position;
            Vector3 forward = eyes.forward;

            float sightRange = sightInfo.range;
            float sightAngle = sightInfo.angle;

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

    [Serializable]
    public struct SightInfo
    {
        public float range;
        public int angle;
    }
}