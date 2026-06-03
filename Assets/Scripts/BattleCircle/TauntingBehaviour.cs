using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    //TODO have an parent class with fightingBehaviour or interfaces
    public class TauntingBehaviour : BattleCircleAi
    {
        private readonly Transform _targetTransform;
        private readonly List<BlackboardReference> _containingAis; 
        
        public TauntingBehaviour(List<BlackboardReference> containingAis, ref UnityAction<BlackboardReference> assigningEvent)
        {
            _containingAis = containingAis;
            
            assigningEvent += AssignAi;
        }
        
        public BlackboardReference[] CheckIfStillActive()
        {
            List<BlackboardReference> result = new List<BlackboardReference>();
            for (int i = 0; i < _containingAis.Count; i++)
            {
                BlackboardReference blackboard = _containingAis[^(i + 1)];
                blackboard.GetVariableValue("AbleToTaunt", out bool tauntValue);

                if (tauntValue)
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
            blackboard.SetVariableValue("AbleToTaunt", true);
            blackboard.GetVariableValue("Self",out GameObject aiObject);
            SetAITransformPoint(blackboard,aiObject.transform);
        }
        
        public void RemoveAi(BlackboardReference blackboard)
        {
            _containingAis.Remove(blackboard);
        }
    }
}