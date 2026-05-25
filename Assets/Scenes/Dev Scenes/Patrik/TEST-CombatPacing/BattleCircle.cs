using System;
using System.Collections.Generic;
using AI_Experimental.Extra;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class BattleCircle : MonoBehaviour
    {
        public UnityAction<BlackboardReference> AssignAsFighting = delegate { };
        public UnityAction<BlackboardReference> AssignAsTaunting = delegate { };
        
        [Space,SerializeField] private BattleCircleData data;
        
        private readonly List<BlackboardReference> _aisInCircle = new ();

        private TokenSystem _tokenSystem;
        private CircleBehaviour _circleBehaviour;
        private FightingBehaviour _fightingBehaviour;
        private TauntingBehaviour _tauntingBehaviour;

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
            _fightingBehaviour = new FightingBehaviour(transform,attackingAis, ref AssignAsFighting);
            
            List<BlackboardReference> tauntingAis = new ();
            _tauntingBehaviour = new TauntingBehaviour(tauntingAis, ref AssignAsTaunting);
            
            _tokenSystem = new TokenSystem(data,_aisInCircle, attackingAis, tauntingAis, ref AssignAsFighting, ref AssignAsTaunting);
        }
        
        private void FixedUpdate()
        {
            UpdateAllEnemiesForward();
        }

        private void Update()
        {
            _tokenSystem.UpdateTime(Time.deltaTime);
            
            _circleBehaviour.AssignInvalidNonWalkablePoints();

            //TODO makes it clunky
            //BlackboardReference[] onBattleCircleTarget = GetAllNoneAttacking();
            //_circleBehaviour.UpdateDynamicTargetPoint(onBattleCircleTarget);
            
            BlackboardReference[] newNonFightingAis = _fightingBehaviour.CheckIfStillActive();
            foreach (var ai in newNonFightingAis)
            {
                _circleBehaviour.ReassignSameTarget(ai);
            }
            
            BlackboardReference[] newNonTauntingAis = _tauntingBehaviour.CheckIfStillActive();
            foreach (var ai in newNonTauntingAis)
            {
                _circleBehaviour.ReassignSameTarget(ai);
            }
        }

        private BlackboardReference[] GetAllNoneAttacking()
        {
            List<BlackboardReference> result = new List<BlackboardReference>();
            foreach (BlackboardReference ai in _aisInCircle)
            {
                if (!_fightingBehaviour.ContainsAI(ai))
                {
                    result.Add(ai);
                }
            }

            return result.ToArray();
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
            if (_fightingBehaviour.ContainsAI(blackboard) || _tauntingBehaviour.ContainsAI(blackboard))
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
            float valueT = MapValues.MapValueToCurve(distanceToTargetPoint, data.forwardPriorityPackage);
            Vector3 blendedDir = Vector3.Slerp(dirToPlayer, dirToTargetPoint,valueT);
            blackboard.SetVariableValue("Forward",blendedDir);
        }
        
        //TODO make external component based on if its connected to battleCircle or not
        /* TODO use later when we have dynamic points
        private void UpdateEnemyForward(BlackboardReference blackboard)
        {
            blackboard.GetVariableValue("Self", out GameObject aiObject);

            Vector3 aiPos = aiObject.transform.position;
            Vector3 playerPos = transform.position;
            Vector3 dirToPlayer = playerPos -aiPos;
            dirToPlayer.y = 0;
            dirToPlayer.Normalize();
            
            Vector2 currentDir = new Vector2(aiObject.transform.forward.x,aiObject.transform.forward.z);
            
            //When attacking there is no need to include target diff because player and target is the same.
            if (_fightingBehaviour.ContainsAI(blackboard))
            {
                Vector2 wantedDir = new Vector2(dirToPlayer.x, dirToPlayer.z);
                Vector2 newDir = SmoothRotation(wantedDir, currentDir,90,Time.deltaTime);
                
                blackboard.SetVariableValue("Forward",newDir);
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
            
            
            Vector2 clampedDir = SmoothRotation(new Vector2(blendedDir.x, blendedDir.z), currentDir,90,Time.deltaTime);
            
            blackboard.SetVariableValue("Forward",new Vector3(clampedDir.x, 0, clampedDir.y));
            //blackboard.SetVariableValue("Forward",dirToPlayer);
        }*/
        
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
            _fightingBehaviour.RemoveAi(blackboard);
            _tauntingBehaviour.RemoveAi(blackboard);
        }
    }
}