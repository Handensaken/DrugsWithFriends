using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class BattleCircle : MonoBehaviour
    {
        public UnityAction<BlackboardReference> AssignAsFighting = delegate { };
        
        [Space,SerializeField] private BattleCircleData data;
        
        private readonly List<BlackboardReference> _aisInCircle = new ();

        private TokenSystem _tokenSystem;
        private CircleBehaviour _circleBehaviour;
        private FightingBehaviour _fightingBehaviour;

        #region Properties

        public List<BlackboardReference> AisInCircle => _aisInCircle;

        public int AmountOfEnemiesInCircle => _aisInCircle.Count; 
        public CircleBehaviour CircleBehaviour => _circleBehaviour;

        #endregion

        private void Awake()
        {
            Dictionary<BlackboardReference, Transform> aiAndTargetTransform = new ();
            _circleBehaviour = new CircleBehaviour(transform,data,aiAndTargetTransform);
            
            List<BlackboardReference> attackingAis = new ();
            _fightingBehaviour = new FightingBehaviour(transform,data,attackingAis, ref AssignAsFighting);
            _tokenSystem = new TokenSystem(data,_aisInCircle, attackingAis, ref AssignAsFighting);
        }
        
        private void FixedUpdate()
        {
            UpdateAllEnemiesForward();
        }

        private void Update()
        {
            //_tokenSystem.UpdateTime(Time.deltaTime);
            
            _circleBehaviour.HandleValidityControlOfTargetPoints();
            
            BlackboardReference[] newNonFightingAis = _fightingBehaviour.CheckIfStillAttacking();
            foreach (var ai in newNonFightingAis)
            {
                _circleBehaviour.ReassignSameTarget(ai);
            }
        }

        private void CheckIfPointsOnNavMesh()
        {
            
        }
        
        private void UpdateAllEnemiesForward()
        {
            foreach (BlackboardReference blackboard in _aisInCircle)
            {
                UpdateEnemyForward(blackboard);
            }
        }
        
        private void UpdateEnemyForward(BlackboardReference blackboard)
        {
            blackboard.GetVariableValue("Self", out GameObject gameObject);
                
            Vector3 forwardDir = transform.position-gameObject.transform.position;
            forwardDir.y = 0;
            forwardDir.Normalize();
                
            blackboard.SetVariableValue("Forward",forwardDir);
        }
        
        public void AssignAI(BlackboardReference blackboard)
        {
            _aisInCircle.Add(blackboard);
            _circleBehaviour.AssignAI(blackboard);
            UpdateEnemyForward(blackboard);
        }
        
        //TODO för när fiender dör eller flyttas
        public void RemoveAI(BlackboardReference blackboard)
        {
            _aisInCircle.Remove(blackboard);
            _circleBehaviour.RemoveAIAndTakenTransform(blackboard);
            _fightingBehaviour.RemoveFightingAi(blackboard);
        }
    }
}