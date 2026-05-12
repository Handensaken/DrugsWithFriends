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
        
        private readonly Dictionary<BlackboardReference, Transform> _aiAndTargetTransform = new (); //TODO structs ??
        private readonly List<BlackboardReference> _attackingAis = new();
        
        //Token creator
        private float _currentTime = 0;

        #region Properties

        public Dictionary<BlackboardReference, Transform> AiAndTargetTransform => _aiAndTargetTransform;

        #endregion
        
        private void Awake()
        {
            SetRndNextTimeForNewFighter();
        }

        private void FixedUpdate()
        {
            UpdateAllEnemiesForward();
        }

        private void Update()
        {
            HandleTokens();
            
            //TODO maybe more suitable i fixedUpdate
            CheckIfAttacking();
        }
        
        private void SetRndNextTimeForNewFighter()
        {
            _currentTime = new Random().Next(data.tokenCreationData.minTime,data.tokenCreationData.maxTime);
            _currentTime += (float)new Random().NextDouble();
        }
        
        private void HandleTokens()
        {
            _currentTime -= Time.deltaTime;
            if (_currentTime <= 0)
            {
                GiveToken();
                SetRndNextTimeForNewFighter();
            }
        }

        private void GiveToken()
        {
            if (_aiAndTargetTransform.Count <= 0)
            {
                return;
            }
            
            Debug.Log("Give Token");
            _attackingAis.Add(_aiAndTargetTransform.Keys.ToArray()[0]);
            _aiAndTargetTransform.Keys.ToArray()[0].SetVariableValue("AbleToAttack", true);
            _aiAndTargetTransform.Keys.ToArray()[0].SetVariableValue("Target", transform);
        }

        private void CheckIfAttacking()
        {
            for (int i = 0; i < _attackingAis.Count; i++)
            {
                BlackboardReference blackboard = _attackingAis[^(i + 1)];
                blackboard.GetVariableValue("AbleToAttack", out bool attackValue);

                if (attackValue)
                {
                    continue;
                }
                
                _attackingAis.RemoveAt((_attackingAis.Count-1)-i);

                blackboard.SetVariableValue("Target",_aiAndTargetTransform[blackboard]);
            }
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
        
        private void SetAITransformPoint(BlackboardReference blackboard, Transform targetTransform)
        {
            blackboard.SetVariableValue("Target",targetTransform);
        }
        
        /// <summary>
        /// Initial enemy base-orientation for first position --> Rotates toward the first enemy
        /// Begins at 0 degrees and continues clockwise (cw) (Left-handed system).
        /// </summary>
        public Vector3[] CreateAllPoints(uint amountOfPoints)
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
                
                Vector2 xzDir = Rotate(new Vector2(forward.x, forward.z).normalized,angleIncrease*i);
                Vector3 pointDir = new Vector3(xzDir.x, 0, xzDir.y);
                result[i] = pointDir * circleRange;
            }

            return result;
        }
        
        private Vector2 Rotate(Vector2 vector, float deltaAngle)
        {
            Vector2 result = new Vector2
                (vector.x*Mathf.Cos(deltaAngle) - vector.y*Mathf.Sin(deltaAngle),
                vector.x*Mathf.Sin(deltaAngle) + vector.y*Mathf.Cos(deltaAngle));
            
            return result;
        }

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
                
                existingPointTransform.position = transform.position+localPoint;
            }
            
            //Tilldela den nya punkten till den nya fienden
            Transform pointTransform = new GameObject().transform;
            pointTransform.parent = transform;
            pointTransform.position = transform.position + localPoints[^1];
                                     
            _aiAndTargetTransform[blackboard] = pointTransform;
            blackboard.SetVariableValue("InBattleCircle", true);
            UpdateEnemyForward(blackboard);
            SetAITransformPoint(blackboard, pointTransform);
        }
        //TODO för när fiender dör eller flyttas
        private void ResignAI2Point(BlackboardReference blackboard)
        {
            
        }
    }
}