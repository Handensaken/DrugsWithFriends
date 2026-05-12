using System.Linq;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class BattleCircleVisualization : MonoBehaviour
    {
        [SerializeField] private BattleCircle battleCircle;
        [SerializeField, Range(1,10), Tooltip("Only for visualization - no logic in game")] private uint amountOfPositioningPoints; //TODO separate to visualizationComponent
        
        [Space,SerializeField] private BattleCircleData data;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, data.circleRange);
            
            if (!Application.isPlaying)
            {
                Vector3[] points = CircleBehaviour.CreateAllPoints(data,amountOfPositioningPoints,battleCircle.transform);
                foreach (Vector3 point in points)
                {
                    Gizmos.DrawSphere(transform.position+point, .2f);
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

            DrawEnemyDir();
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