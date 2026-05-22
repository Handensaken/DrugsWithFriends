using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    [Serializable]
    public struct AngleSpanPackage
    {
        [Range(0,360)]public uint angleStart;
        [Range(0,360)]public uint angleEnd;
    }
    
    public class BattleCircleVisualization : MonoBehaviour
    {
        [SerializeField] private BattleCircle battleCircle;
        [SerializeField] private BattleCircleData battleCircleData;
        [SerializeField, Tooltip("Only for visualization - no logic in game")] private AngleSpanPackage[] allAngleSpans;
        [SerializeField] private bool seAngleSpan;
        
        [Space,SerializeField] private BattleCircleData data;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, data.circleRadius);
            
            DrawAllPoints();
            HandleAllAnglePoints();
            DrawEnemyDir();
        }
        
        private void HandleAllAnglePoints()
        {
            Gizmos.color = Color.yellow;
            
            if (seAngleSpan && !Application.isPlaying)
            {
                foreach (var angleSpan in allAngleSpans)
                {
                    DrawAnglePoints(angleSpan, transform);
                }
            }
            else if (seAngleSpan)
            {
                foreach (var angleSpan in battleCircle.CircleBehaviour.AllCircleOverrides)
                {
                    DrawAnglePoints(angleSpan, transform);
                }
            }
        }

        private void DrawAnglePoints(AngleSpanPackage angleSpan, Transform transform)
        {
            Vector2 xzDir = VectorMath.Rotate(new Vector2(transform.forward.x, transform.forward.z).normalized,
                angleSpan.angleStart *Mathf.Deg2Rad);
            Vector3 pointDir = new Vector3(xzDir.x, 0, xzDir.y);

            Vector3 start = transform.position + pointDir * data.circleRadius;
                    
            xzDir = VectorMath.Rotate(new Vector2(transform.forward.x, transform.forward.z).normalized,angleSpan.angleEnd*Mathf.Deg2Rad);
            pointDir = new Vector3(xzDir.x, 0, xzDir.y);

            Vector3 end = transform.position + pointDir * data.circleRadius;
            Gizmos.DrawLine(start, end);
        }
        
        private void DrawAllPoints()
        {
            if (!Application.isPlaying)
            {
                //TODO
                BattleCirclePointPackage[] allPointPackages = CircleBehaviour.CreateAllPointsPackages(
                    data,battleCircleData.amountOfPointsInCircle,battleCircle.transform);
                
                BattleCirclePointPackage[] validInAnglePointPackages = CircleBehaviour.FindOutOfAngleCirclePoints(
                    allPointPackages,allAngleSpans, out BattleCirclePointPackage[] invalidInAnglePointPackages);
                
                //Draw InvalidAngle
                Gizmos.color = Color.yellow;
                DrawPoints(invalidInAnglePointPackages);
                
                BattleCirclePointPackage[] walkablePointPackages = CircleBehaviour.FindAllWalkablePoints(
                    transform.position, transform.position, validInAnglePointPackages,out BattleCirclePointPackage[] invalidWalkablePointPackages);
                
                //Draw NotWalkable
                Gizmos.color = Color.orangeRed;
                DrawPoints(invalidWalkablePointPackages);
                
                //Draw valid points
                Gizmos.color = Color.darkBlue;
                DrawPoints(walkablePointPackages);
            }
            else
            {
                BattleCirclePointPackage[] allPointPackages = CircleBehaviour.CreateAllPointsPackages(
                    data,battleCircleData.amountOfPointsInCircle,battleCircle.transform);
                
                BattleCirclePointPackage[] validInAnglePointPackages = CircleBehaviour.FindOutOfAngleCirclePoints(
                    allPointPackages,battleCircle.CircleBehaviour.AllCircleOverrides, out BattleCirclePointPackage[] invalidInAnglePointPackages);
                
                //Draw InvalidAngle
                Gizmos.color = Color.yellow;
                DrawPoints(invalidInAnglePointPackages);
                
                BattleCirclePointPackage[] walkablePointPackages = CircleBehaviour.FindAllWalkablePoints(
                    transform.position, transform.position, validInAnglePointPackages,out BattleCirclePointPackage[] invalidWalkablePointPackages);
                
                //Draw NotWalkable
                Gizmos.color = Color.orangeRed;
                DrawPoints(invalidWalkablePointPackages);
                
                //Draw taken points
                Gizmos.color = Color.darkGreen;
                Transform[] takenPoints = battleCircle.CircleBehaviour.AisAndTakenTransforms.Values.ToArray();
                foreach (Transform takenPoint in takenPoints)
                {
                    Gizmos.DrawSphere(takenPoint.position, .2f);
                }
                
                //TODO handle change of available transforms
                /*//Draw available points
                Gizmos.color = Color.darkBlue;
                Transform[] availablePoints = battleCircle.CircleBehaviour.AvailableTransforms.ToArray();
                foreach (Transform availablePoint in availablePoints)
                {
                    Gizmos.DrawSphere(availablePoint.position, .2f);
                }*/
            }
        }

        private void DrawPoints(BattleCirclePointPackage[] pointsPackages)
        {
            foreach (var point in pointsPackages)
            {
                Gizmos.DrawSphere(transform.position+point.PointInCircle, .2f);
            }
        }
        
        private void DrawEnemyDir()
        {
            foreach (BlackboardReference blackboard in battleCircle.AisInCircle)
            {
                blackboard.GetVariableValue("Self", out GameObject gameObject);
                
                Vector3 forwardDir = transform.position - gameObject.transform.position;
                forwardDir.y = 0;
                forwardDir.Normalize();
                
                Gizmos.DrawSphere(gameObject.transform.position+forwardDir*2,.5f);
            }
        }
    }
}