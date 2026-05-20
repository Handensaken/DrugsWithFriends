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
        [SerializeField, Tooltip("Only for visualization - no logic in game")] private AngleSpanPackage[] allAngleSpans;
        [SerializeField] private bool seAngleSpan;
        
        [Space,SerializeField] private BattleCircleData data;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, data.circleRange);
            
            DrawPoints();
            HandleAllAnglePoints();
            DrawEnemyDir();
        }

        private void HandleAllAnglePoints()
        {
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

            Vector3 start = transform.position + pointDir * data.circleRange;
                    
            xzDir = VectorMath.Rotate(new Vector2(transform.forward.x, transform.forward.z).normalized,angleSpan.angleEnd*Mathf.Deg2Rad);
            pointDir = new Vector3(xzDir.x, 0, xzDir.y);

            Vector3 end = transform.position + pointDir * data.circleRange;
            Gizmos.DrawLine(start, end);
        }
        
        private void DrawPoints()
        {
            if (!Application.isPlaying)
            {
                BattleCirclePointPackage[] pointPackages = CircleBehaviour.CreateAllPointsPackages(data,amountOfPositioningPoints,battleCircle.transform);
                DrawValidPoints(pointPackages, allAngleSpans);
            }
            else
            {
                Transform[] pointTransforms = battleCircle.CircleBehaviour.AisAndTakenTransforms.Values.ToArray();
                foreach (Transform point in pointTransforms)
                {
                    Gizmos.DrawSphere(point.position, .2f);
                }
                
                //DrawValidPoints(pointPackages);
            }
        }

        private void DrawValidPoints(BattleCirclePointPackage[] pointPackages, AngleSpanPackage[] angleSpanPackages)
        {
            BattleCirclePointPackage[] points = CircleBehaviour.FindAllValidCirclePoints(pointPackages,angleSpanPackages);

            foreach (var validPoint in points)
            {
                Gizmos.DrawSphere(transform.position+validPoint.PointInCircle, .2f);
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