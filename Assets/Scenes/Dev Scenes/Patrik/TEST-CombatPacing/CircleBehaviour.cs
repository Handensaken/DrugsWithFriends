using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class CircleBehaviour : BattleCircleAi
    {
        private readonly Transform _battleCircleTransform;
        private readonly BattleCircleData _data;
        private readonly Dictionary<BlackboardReference, Transform> _aisAndTargetTransforms;
        
        public CircleBehaviour(Transform battleCircleTransform,BattleCircleData data,Dictionary<BlackboardReference, Transform> aisAndTargetTransforms)
        {
            _battleCircleTransform = battleCircleTransform;
            _data = data;
            _aisAndTargetTransforms = aisAndTargetTransforms;
        }

        public Dictionary<BlackboardReference, Transform> AisAndTargetTransforms => _aisAndTargetTransforms;
        
        public void UpdateNewNonFightingTargets(BlackboardReference[] newNonFightingAis)
        {
            foreach (var blackboard in newNonFightingAis)
            {
                Debug.Log(_aisAndTargetTransforms.ContainsKey(blackboard));
                Transform target = _aisAndTargetTransforms[blackboard];
                blackboard.SetVariableValue("Target", target);
                Debug.Log("Here");
            }
        }
        
        /// <summary>
        /// Initial enemy base-orientation for first position --> Rotates toward the first enemy
        /// Begins at 0 degrees and continues clockwise (cw) (Left-handed system).
        /// </summary>
        public static Vector3[] CreateAllPoints(BattleCircleData data, uint amountOfPoints, Transform transform)
        {
            float circleRange = data.circleRange;
            
            if (amountOfPoints <= 0)
            {
                return null;
            }
            
            if (amountOfPoints == 1)
            {
                return new[] { transform.forward*circleRange};
            }
            
            Vector3[] result = new Vector3[amountOfPoints];

            Vector3 forward = transform.forward;
            float angleIncrease = Mathf.Deg2Rad*(360f / amountOfPoints);
            for (int i = 0; i < amountOfPoints; i++)
            {
                if (i == 0)
                {
                    result[i] = transform.position + forward * circleRange;
                }
                
                Vector2 xzDir = VectorMath.Rotate(new Vector2(forward.x, forward.z).normalized,angleIncrease*i);
                Vector3 pointDir = new Vector3(xzDir.x, 0, xzDir.y);
                result[i] = pointDir * circleRange;
            }

            return result;
        }
        
        public void AssignAI2Point(BlackboardReference blackboard)
        {
            //Skapa nya positionsPunkter
            uint amountOfPoints = (uint)(_aisAndTargetTransforms.Count+1);
            Vector3[] localPoints = CreateAllPoints(_data,amountOfPoints, _battleCircleTransform);
            
            //Uppdatera positionen på transforms
            for (int i = 0; i < _aisAndTargetTransforms.Values.Count; i++)
            {
                Vector3 localPoint = localPoints[i];
                Transform existingPointTransform = _aisAndTargetTransforms.Values.ToArray()[i];
                
                existingPointTransform.position = _battleCircleTransform.position+localPoint;
            }
            
            //Tilldela den nya punkten till den nya fienden
            Transform pointTransform = new GameObject().transform;
            pointTransform.parent = _battleCircleTransform;
            pointTransform.position = _battleCircleTransform.position + localPoints[^1];
                                     
            _aisAndTargetTransforms[blackboard] = pointTransform;
            blackboard.SetVariableValue("InBattleCircle", true);
            SetAITransformPoint(blackboard, pointTransform);
        }
        
        private void ResignAI2Point(BlackboardReference blackboard)
        {
            
        }
    }
}