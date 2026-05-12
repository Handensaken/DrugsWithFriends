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
        private readonly Dictionary<BlackboardReference, Transform> _aiAndTargetTransform;
        
        public CircleBehaviour(Transform battleCircleTransform,BattleCircleData data,Dictionary<BlackboardReference, Transform> aiAndTargetTransform)
        {
            _battleCircleTransform = battleCircleTransform;
            _data = data;
            _aiAndTargetTransform = aiAndTargetTransform;
        }
        
        public void UpdateNewNonFightingTargets(BlackboardReference[] newNonFightingAis)
        {
            foreach (var blackboard in newNonFightingAis)
            {
                blackboard.SetVariableValue("Target",_aiAndTargetTransform[blackboard]);
            }
        }
        
        //CircleBehaviour
        /// <summary>
        /// Initial enemy base-orientation for first position --> Rotates toward the first enemy
        /// Begins at 0 degrees and continues clockwise (cw) (Left-handed system).
        /// </summary>
        public Vector3[] CreateAllPoints(uint amountOfPoints)
        {
            float circleRange = _data.circleRange;
            
            if (amountOfPoints <= 0)
            {
                return null;
            }
            
            if (amountOfPoints == 1)
            {
                return new[] { _battleCircleTransform.forward*circleRange};
            }
            
            Vector3[] result = new Vector3[amountOfPoints];

            Vector3 forward = _battleCircleTransform.forward;
            float angleIncrease = Mathf.Deg2Rad*(360f / amountOfPoints);
            for (int i = 0; i < amountOfPoints; i++)
            {
                if (i == 0)
                {
                    result[i] = _battleCircleTransform.position + forward * circleRange;
                }
                
                Vector2 xzDir = VectorMath.Rotate(new Vector2(forward.x, forward.z).normalized,angleIncrease*i);
                Vector3 pointDir = new Vector3(xzDir.x, 0, xzDir.y);
                result[i] = pointDir * circleRange;
            }

            return result;
        }
        
        //CircleBehaviour
        public void AssignAI2Point(BlackboardReference blackboard)
        {
            //Skapa nya positionsPunkter
            uint amountOfPoints = (uint)(_aiAndTargetTransform.Count+1);
            Vector3[] localPoints = CreateAllPoints(amountOfPoints);
            
            //Uppdatera positionen på transforms
            for (int i = 0; i < _aiAndTargetTransform.Values.Count; i++)
            {
                Vector3 localPoint = localPoints[i];
                Transform existingPointTransform = _aiAndTargetTransform.Values.ToArray()[i];
                
                existingPointTransform.position = _battleCircleTransform.position+localPoint;
            }
            
            //Tilldela den nya punkten till den nya fienden
            Transform pointTransform = new GameObject().transform;
            pointTransform.parent = _battleCircleTransform;
            pointTransform.position = _battleCircleTransform.position + localPoints[^1];
                                     
            _aiAndTargetTransform[blackboard] = pointTransform;
            blackboard.SetVariableValue("InBattleCircle", true);
            SetAITransformPoint(blackboard, pointTransform);
        }
        
        //CircleBehaviour
        
        private void ResignAI2Point(BlackboardReference blackboard)
        {
            
        }
    }
}