using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class BattleCircle : MonoBehaviour
    {
        [Space,SerializeField] private BattleCircleData data;
        
        //Borde endast vara Lista med AI-blackboard
        //Resterande borde finnas i respektive beteende
        private readonly Dictionary<BlackboardReference, Transform> _aiAndTargetTransform = new (); //TODO structs ??
        private readonly List<BlackboardReference> _attackingAis = new();

        private TokenSystem _tokenSystem;
        private CircleBehaviour _circleBehaviour;
        private FightingBehaviour _fightingBehaviour;

        #region Properties

        public Dictionary<BlackboardReference, Transform> AiAndTargetTransform => _aiAndTargetTransform;
        public CircleBehaviour CircleBehaviour => _circleBehaviour;

        #endregion
        
        private void Awake()
        {
            _tokenSystem = new TokenSystem(data);
            _circleBehaviour = new CircleBehaviour(transform,data,_aiAndTargetTransform);
            _fightingBehaviour = new FightingBehaviour(transform,data,_attackingAis);
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
            foreach (BlackboardReference blackboard in _aiAndTargetTransform.Keys)
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
            _circleBehaviour.AssignAI2Point(blackboard);
            UpdateEnemyForward(blackboard);
        }
        
        //TODO för när fiender dör eller flyttas
        public void ResignAI(BlackboardReference blackboard)
        {
            
        }
        
        
    }
}