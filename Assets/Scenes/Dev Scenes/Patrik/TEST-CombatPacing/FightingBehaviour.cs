using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class FightingBehaviour : BattleCircleAi
    {
        
        private readonly BattleCircleData _data;
        private readonly Transform _battleCircleTransform;
        private readonly List<BlackboardReference> _attackingAis; 
        
        public FightingBehaviour(Transform battleCircleTransform,BattleCircleData data,List<BlackboardReference> attackingAis, ref UnityAction<BlackboardReference> attackingEvent)
        {
            _battleCircleTransform = battleCircleTransform;
            
            _data = data;
            attackingEvent += AssignFightingAi;
            
            _attackingAis = attackingAis;
        }
        
        public BlackboardReference[] CheckIfStillAttacking()
        {
            List<BlackboardReference> result = new List<BlackboardReference>();
            for (int i = 0; i < _attackingAis.Count; i++)
            {
                BlackboardReference blackboard = _attackingAis[^(i + 1)];
                blackboard.GetVariableValue("AbleToAttack", out bool attackValue);

                if (attackValue)
                {
                    continue;
                }
                
                _attackingAis.RemoveAt((_attackingAis.Count-1)-i);
                result.Add(blackboard);
            }

            return result.ToArray();
        }

        public bool ContainsAI(BlackboardReference ai)
        {
            return _attackingAis.Contains(ai);
        } 
        
        private void AssignFightingAi(BlackboardReference blackboard)
        {
            _attackingAis.Add(blackboard);
            blackboard.SetVariableValue("AbleToAttack", true);
            SetAITransformPoint(blackboard,_battleCircleTransform);
        }
        
        public void RemoveFightingAi(BlackboardReference blackboard)
        {
            _attackingAis.Remove(blackboard);
        }
    }
}