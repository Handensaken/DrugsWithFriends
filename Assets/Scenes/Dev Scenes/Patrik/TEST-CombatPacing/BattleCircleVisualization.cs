using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField, Range(1,10), Tooltip("Only for visualization - no logic in game")] private uint amountOfPositioningPoints;
        [SerializeField] private AngleSpanPackage[] allAngleSpans;
        [SerializeField] private bool seAngleSpan;
        
        [Space,SerializeField] private BattleCircleData data;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, data.circleRange);
            
            DrawPoints();
            DrawAnglePoints();
            DrawEnemyDir();
        }

        private void DrawAnglePoints()
        {
            if (seAngleSpan && !Application.isPlaying)
            {
                foreach (var angleSpan in allAngleSpans)
                {
                    Vector2 xzDir = VectorMath.Rotate(new Vector2(transform.forward.x, transform.forward.z).normalized,
                        angleSpan.angleStart *Mathf.Deg2Rad);
                    Vector3 pointDir = new Vector3(xzDir.x, 0, xzDir.y);

                    Vector3 start = transform.position + pointDir * data.circleRange;
                    
                    xzDir = VectorMath.Rotate(new Vector2(transform.forward.x, transform.forward.z).normalized,angleSpan.angleEnd*Mathf.Deg2Rad);
                    pointDir = new Vector3(xzDir.x, 0, xzDir.y);

                    Vector3 end = transform.position + pointDir * data.circleRange;
                    Gizmos.DrawLine(start, end);
                }
            }
            else if (seAngleSpan)
            {
                
                
            }
        }
        
        private void DrawPoints()
        {
            if (!Application.isPlaying)
            {
                CircleBehaviour.BattleCirclePointPackage[] pointPackages = CircleBehaviour.CreateAllPointsPackages(data,amountOfPositioningPoints,battleCircle.transform);
                
                List<CircleBehaviour.BattleCirclePointPackage> points = pointPackages.ToList();
                
                foreach (var angleSpan in allAngleSpans)
                {
                    uint angleStart = angleSpan.angleStart;
                    uint angleEnd = angleSpan.angleEnd;
                    List<CircleBehaviour.BattleCirclePointPackage> validPoints = new();
                    foreach (var validatingPoint in points)
                    {
                        
                        if (!IsWithinAngles(validatingPoint.AngleInCircle, angleStart, angleEnd))
                        {
                            validPoints.Add(validatingPoint);
                        }
                    }

                    points = validPoints;
                }

                foreach (var validPoint in points)
                {
                    Gizmos.DrawSphere(transform.position+validPoint.PointInCircle, .2f);
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
                return pointAngle >= angleStart || pointAngle <= angleEnd;
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