using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class BattleCircle : MonoBehaviour
    {
        public UnityAction<BlackboardReference> AssignAsFighting = delegate { };
        
        [SerializeField] private int clientID;
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

        public void SetUpBattleCircle(int clientID)
        {
            this.clientID = clientID;
            
            Dictionary<BlackboardReference, Transform> aiAndTargetTransform = new ();
            _circleBehaviour = new CircleBehaviour(transform,data,aiAndTargetTransform);
            
            List<BlackboardReference> attackingAis = new ();
            _fightingBehaviour = new FightingBehaviour(transform,data,attackingAis, ref AssignAsFighting);
            Debug.LogWarning("Awake: "+transform.name);
            _tokenSystem = new TokenSystem(data,_aisInCircle, attackingAis, ref AssignAsFighting);
        }
        
        private void FixedUpdate()
        {
            UpdateAllEnemiesForward();
        }

        private void Update()
        {
            _tokenSystem.UpdateTime(Time.deltaTime);
            
            BlackboardReference[] newNonFightingAis = _fightingBehaviour.CheckIfStillAttacking();
            _circleBehaviour.UpdateNewNonFightingTargets(newNonFightingAis);
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
            _circleBehaviour.AssignAI2Point(blackboard);
            UpdateEnemyForward(blackboard);
        }
        
        //TODO för när fiender dör eller flyttas
        public void ResignAI(BlackboardReference blackboard)
        {
            
        }
        
        
    }
}