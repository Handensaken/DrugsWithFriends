using System;
using StateMachine.Solid.ScriptableObjects;
using UnityEngine;

namespace StateMachine.Solid.Scripts.States
{
    public class SeeingStateVisualizer : BaseStateVisualizer
    {
        private readonly ISightData sightData;
        
        public SeeingStateVisualizer(ISightData sightData, Color stateColor, Transform sightTransform) : base(stateColor, sightTransform)
        {
            this.sightData = sightData;
        }

        public override void Visualize()
        {
            //Color from parentClass
            base.Visualize();
            VisualizeSightRange();
            VisualizeSightAngle();
        }
        
        private void VisualizeSightRange()
        {
            Gizmos.DrawWireSphere(SightTransform.position, sightData.SightRange);
        }
        
        private void VisualizeSightAngle() //Shame
        {
            //Only need x, z
            Vector3 worldPos = SightTransform.position;
            Vector3 forward = SightTransform.forward;

            //LeftSide
            Vector2 valuesForLeftSide = RotateVectorCounter(new Vector2(forward.x,forward.z), sightData.SightAngle);
            Vector3 leftSide = new Vector3(valuesForLeftSide.x, 0, valuesForLeftSide.y)*sightData.SightRange;

            Vector3 leftCubePos = worldPos + leftSide;
            Gizmos.DrawLine(worldPos, leftCubePos);
            Gizmos.DrawCube(leftCubePos, new Vector3(.1f,.1f,.1f));
        
            
            //RightSide
            Vector2 valuesForRightSide = RotateVectorClock(new Vector2(forward.x,forward.z), sightData.SightAngle);
            Vector3 rightSide = new Vector3(valuesForRightSide.x, 0, valuesForRightSide.y)*sightData.SightRange;

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