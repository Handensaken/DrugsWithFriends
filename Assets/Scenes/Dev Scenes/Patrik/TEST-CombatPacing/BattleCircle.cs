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
        [SerializeField, Range(1,10), Tooltip("Only for visualization - no logic in game")] private uint amountOfPositioningPoints; //TODO separate to visualizationComponent
        [SerializeField,Min(0.1f)] private float circleRange;

        private readonly Dictionary<BlackboardReference, Transform> _aiPointDictionary = new (); //TODO structs ??

        private readonly List<BlackboardReference> _currentlyAttacking = new();
        
        //Token creator
        [Space, SerializeField,Range(0,3)] private int minTime;
        [Space, SerializeField,Range(0,5)] private int maxTime;
        private float _currentTime = 0;

        private void Awake()
        {
            //TEMP
            _currentTime = 0;
            _currentTime += new Random().Next(minTime,maxTime);
            _currentTime += (float)new Random().NextDouble();
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

        private void HandleTokens()
        {
            _currentTime -= Time.deltaTime;
            if (_currentTime <= 0)
            {
                GiveToken();
                
                _currentTime = 0;
                _currentTime += new Random().Next(minTime,maxTime);
                _currentTime += (float)new Random().NextDouble();
            }
        }

        private void GiveToken()
        {
            if (_aiPointDictionary.Count <= 0)
            {
                return;
            }
            
            Debug.Log("Give Token");
            _currentlyAttacking.Add(_aiPointDictionary.Keys.ToArray()[0]);
            _aiPointDictionary.Keys.ToArray()[0].SetVariableValue("AbleToAttack", true);
            _aiPointDictionary.Keys.ToArray()[0].SetVariableValue("Target", transform);
        }

        private void CheckIfAttacking()
        {
            for (int i = 0; i < _currentlyAttacking.Count; i++)
            {
                BlackboardReference blackboard = _currentlyAttacking[^(i + 1)];
                blackboard.GetVariableValue("AbleToAttack", out bool attackValue);

                if (attackValue)
                {
                    continue;
                }
                
                _currentlyAttacking.RemoveAt((_currentlyAttacking.Count-1)-i);

                blackboard.SetVariableValue("Target",_aiPointDictionary[blackboard]);
            }
        }
        
        private void UpdateAllEnemiesForward()
        {
            foreach (BlackboardReference blackboard in _aiPointDictionary.Keys)
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
        private Vector3[] CreateAllPoints(uint amountOfPoints)
        {
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
            uint amountOfPoints = (uint)(_aiPointDictionary.Count+1);
            Vector3[] localPoints = CreateAllPoints(amountOfPoints);
            
            //Uppdatera positionen på transforms
            for (int i = 0; i < _aiPointDictionary.Values.Count; i++)
            {
                Vector3 localPoint = localPoints[i];
                Transform existingPointTransform = _aiPointDictionary.Values.ToArray()[i];
                
                existingPointTransform.position = transform.position+localPoint;
            }
            
            //Tilldela den nya punkten till den nya fienden
            Transform pointTransform = new GameObject().transform;
            pointTransform.parent = transform;
            pointTransform.position = transform.position + localPoints[^1];
                                     
            _aiPointDictionary[blackboard] = pointTransform;
            blackboard.SetVariableValue("InBattleCircle", true);
            UpdateEnemyForward(blackboard);
            SetAITransformPoint(blackboard, pointTransform);
        }
        //TODO för när fiender dör eller flyttas
        private void ResignAI2Point(BlackboardReference blackboard)
        {
            
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, circleRange);
            
            if (!Application.isPlaying)
            {
                Vector3[] points = CreateAllPoints(amountOfPositioningPoints);
                foreach (Vector3 point in points)
                {
                    Gizmos.DrawSphere(transform.position+point, .2f);
                }
                
            }
            else
            {
                foreach (Transform point in _aiPointDictionary.Values)
                {
                    Gizmos.DrawSphere(point.position, .2f);
                }
            }

            DrawEnemyDir();
        }

        private void DrawEnemyDir()
        {
            foreach (BlackboardReference blackboard in _aiPointDictionary.Keys)
            {
                blackboard.GetVariableValue("Self", out GameObject gameObject);
                
                Vector3 forwardDir = transform.position - gameObject.transform.position;
                forwardDir.y = 0;
                forwardDir.Normalize();
                
                Gizmos.DrawSphere(gameObject.transform.position+forwardDir*2,.5f);
            }
        }
    }
}