using System;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    [Serializable]
    public struct AngleSpanPackage
    {
        [Range(0,359)]public uint angleStart;
        [Range(0,359)]public uint angleEnd;
    }
    
    public class BattleCircleVisualization : MonoBehaviour
    {
        [SerializeField] private BattleCircle battleCircle;
        [SerializeField, Range(1,10), Tooltip("Only for visualization - no logic in game")] private uint amountOfPositioningPoints;
        [SerializeField] private AngleSpanPackage[] allAngleSpans;
        
        [Space,SerializeField] private BattleCircleData data;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, data.circleRange);
            
            DrawPoints();

            DrawEnemyDir();
        }
        
        private void DrawPoints()
        {
            if (!Application.isPlaying)
            {
                CircleBehaviour.BattleCirclePointPackage[] pointPackages = CircleBehaviour.CreateAllPointsPackages(data,amountOfPositioningPoints,battleCircle.transform);
                
                uint angleStart = allAngleSpans[0].angleStart;
                uint angleEnd = allAngleSpans[0].angleEnd;
                
                foreach (var pointPackage in pointPackages)
                {
                    if (IsWithinAngles(pointPackage.AngleInCircle, angleStart, angleEnd))
                    {
                        Debug.Log("Draws");
                        Gizmos.DrawSphere(transform.position+pointPackage.PointInCircle, .2f);
                    }
                }
            }
            else
            {
                Transform[] pointTransforms = battleCircle.CircleBehaviour.AisAndTargetTransforms.Values.ToArray();
                foreach (Transform point in pointTransforms)
                {
                    Gizmos.DrawSphere(point.position, .2f);
                }
            }
        }

        private bool IsWithinAngles(float pointAngle, uint angleStart, uint angleEnd)
        {
            Debug.Log(pointAngle);
            
            if (Mathf.Approximately(angleStart, angleEnd))
            {
                Debug.Log("Same angleStart and angleEnd value --> No point can be used");
                return false;
            }
            
            if (angleStart < angleEnd)
            {
                return pointAngle >= angleStart && pointAngle <= angleEnd;
            }
            else
            {
                return pointAngle >= angleEnd || pointAngle <= angleStart;
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