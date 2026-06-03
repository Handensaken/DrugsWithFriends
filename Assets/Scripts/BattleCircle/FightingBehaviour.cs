using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class FightingBehaviour : BattleCircleAi
    {
        private readonly Transform _targetTransform;
        private readonly List<BlackboardReference> _containingAis; 
        
        public FightingBehaviour(Transform targetTransform,List<BlackboardReference> containingAis, ref UnityAction<BlackboardReference> attackingEvent)
        {
            _targetTransform = targetTransform;
            _containingAis = containingAis;
            
            attackingEvent += AssignAi;
        }
        
        public BlackboardReference[] CheckIfStillActive()
        {
            List<BlackboardReference> result = new List<BlackboardReference>();
            for (int i = 0; i < _containingAis.Count; i++)
            {
                BlackboardReference blackboard = _containingAis[^(i + 1)];
                blackboard.GetVariableValue("AbleToAttack", out bool attackValue);

                if (attackValue)
                {
                    continue;
                }
                
                _containingAis.RemoveAt((_containingAis.Count-1)-i);
                result.Add(blackboard);
            }

            return result.ToArray();
        }

        public bool ContainsAI(BlackboardReference ai)
        {
            return _containingAis.Contains(ai);
        } 
        
        private void AssignAi(BlackboardReference blackboard)
        {
            _containingAis.Add(blackboard);
            blackboard.SetVariableValue("AbleToAttack", true);
            SetAITransformPoint(blackboard,_targetTransform);
        }
        
        public void RemoveAi(BlackboardReference blackboard)
        {
            _containingAis.Remove(blackboard);
        }
    }
}