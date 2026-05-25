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
            
            _circleBehaviour.AssignInvalidNonWalkablePoints();
            
            BlackboardReference[] newNonFightingAis = _fightingBehaviour.CheckIfStillAttacking();
            foreach (var ai in newNonFightingAis)
            {
                _circleBehaviour.ReassignSameTarget(ai);
            }
        }
        
        private void UpdateAllEnemiesForward()
        {
            foreach (BlackboardReference blackboard in _aisInCircle)
            {
                UpdateEnemyForward(blackboard);
            }
        }
        
        //TODO make external component based on if its connected to battleCircle or not
        private void UpdateEnemyForward(BlackboardReference blackboard)
        {
            blackboard.GetVariableValue("Self", out GameObject aiObject);

            Vector3 aiPos = aiObject.transform.position;
            Vector3 playerPos = transform.position;
            Vector3 dirToPlayer = playerPos -aiPos;
            dirToPlayer.y = 0;
            dirToPlayer.Normalize();
            
            //When attacking there is no need to include target diff because player and target is the same.
            if (_fightingBehaviour.ContainsAI(blackboard))
            {
                blackboard.SetVariableValue("Forward",dirToPlayer);
                return;
            }
            
            //Now there is a diff between target and player when the enemy is trying to focus on player while running to targetPoint
            Vector3 targetPointPos = _circleBehaviour.AisAndTakenTransforms[blackboard].position;
            Vector3 dirToTargetPoint = targetPointPos - aiPos;
            dirToTargetPoint.y = 0;
            dirToTargetPoint.Normalize();
            
            float distanceToTargetPoint = Vector3.Distance(aiPos, targetPointPos);
            //TODO include curve!!
            Vector3 blendedDir = Vector3.Slerp(dirToPlayer, dirToTargetPoint,distanceToTargetPoint);
            
            blackboard.SetVariableValue("Forward",blendedDir);
        }
        
        public void AssignAI(BlackboardReference blackboard)
        {
            _aisInCircle.Add(blackboard);
            _circleBehaviour.AssignAI(blackboard);
            UpdateEnemyForward(blackboard);
        }
        
        public void RemoveAI(BlackboardReference blackboard)
        {
            Debug.Log("Remove in battle circle");
            _aisInCircle.Remove(blackboard);
            _circleBehaviour.RemoveAIAndTakenTransform(blackboard);
            _fightingBehaviour.RemoveFightingAi(blackboard);
        }
    }
}