using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class CircleBehaviour : BattleCircleAi
    {
        private readonly Transform _battleCircleTransform;
        private readonly BattleCircleData _data;
        private readonly Dictionary<BlackboardReference, Transform> _aisAndTargetTransforms;

        private readonly BattleCirclePointPackage[] _allPoints;
        private BattleCirclePointPackage[] _validPoints;
        private AngleSpanPackage[] _allCircleOverrides = Array.Empty<AngleSpanPackage>(); 
        
        public CircleBehaviour(Transform battleCircleTransform,BattleCircleData data,Dictionary<BlackboardReference, Transform> aisAndTargetTransforms)
        {
            _battleCircleTransform = battleCircleTransform;
            _data = data;
            _aisAndTargetTransforms = aisAndTargetTransforms;
            
            //Test
            uint amountOfPoints = 20; //TODO parameter
            _allPoints = CreateAllPointsPackages(_data,amountOfPoints, _battleCircleTransform);
            _validPoints = _allPoints.ToArray();
        }

        public Dictionary<BlackboardReference, Transform> AisAndTargetTransforms => _aisAndTargetTransforms;

        public AngleSpanPackage[] AllCircleOverrides
        {
            get => _allCircleOverrides;
            set
            {
                _allCircleOverrides = value;
                HandleChangeOfValidPoints();
            }
        }
        
        private void HandleChangeOfValidPoints()
        {
            _validPoints = FindAllValidCirclePoints(_allPoints, _allCircleOverrides);
            if (!FindAiWithInvalidTargets(out BlackboardReference[] aiWithoutValidTargets, out Transform[] takenTargets))
            {
                return;
            }

            if (!FindAvailablePoints(takenTargets, out BattleCirclePointPackage[] points))
            {
                //TODO Send out to other circles
                //TODO Just taunt??
            }
            
            //TODO assiagne


        }

        private bool FindAvailablePoints(Transform[] takenTransforms,out BattleCirclePointPackage[] availablePoints)
        {
            List<BattleCirclePointPackage> result = new List<BattleCirclePointPackage>();
            foreach (var pointPackage in _validPoints)
            {
                bool availableFlag = true;
                foreach (var taken in takenTransforms)
                {
                    Vector3 pointRelativeToCenter = taken.position - _battleCircleTransform.position;
                    if (pointRelativeToCenter == taken.position)
                    {
                        availableFlag = false;
                        break;
                    }
                }

                if (availableFlag)
                {
                    result.Add(pointPackage);
                }
            }

            availablePoints = result.ToArray();
            return availablePoints.Length > 0;
        }
        
        private bool FindAiWithInvalidTargets(out BlackboardReference[] hasNoValidTarget, out Transform[] allTakenTransforms)
        {
            List<BlackboardReference> resultMissingValid = new List<BlackboardReference>();
            List<Transform> resultTakenTransforms = new List<Transform>();
            foreach (var aiAndTarget in _aisAndTargetTransforms)
            {
                bool validFlag = false;
                foreach (var pointPackage in _validPoints)
                {
                    Vector3 pointRelativeToCenter = aiAndTarget.Value.position - _battleCircleTransform.position;  
                    if ( pointRelativeToCenter == pointPackage.PointInCircle)
                    {
                        Debug.Log("Samma - Kan nu raderas eftersom den fungerar");
                        validFlag = true;
                        break;
                    }
                }

                if (validFlag)
                {
                    resultMissingValid.Add(aiAndTarget.Key);
                }
                else
                {
                    resultTakenTransforms.Add(aiAndTarget.Value);
                }
            }

            hasNoValidTarget = resultMissingValid.ToArray();
            allTakenTransforms = resultTakenTransforms.ToArray();
            return hasNoValidTarget.Length > 0;
        }
        
        public void UpdateNewNonFightingTargets(BlackboardReference[] newNonFightingAis)
        {
            foreach (var blackboard in newNonFightingAis)
            {
                Transform target = _aisAndTargetTransforms[blackboard];
                blackboard.SetVariableValue("Target", target);
            }
        }
        /// <summary>
        /// Initial enemy base-orientation for first position --> Rotates toward the first enemy
        /// Begins at 0 degrees and continues clockwise (cw) (Left-handed system).
        /// </summary>
        public static BattleCirclePointPackage[] CreateAllPointsPackages(BattleCircleData data, uint amountOfPoints, Transform transform)
        {
            Vector3 forward = transform.forward;
            float circleRange = data.circleRange;
            
            if (amountOfPoints <= 0)
            {
                return null;
            }
            BattleCirclePointPackage[] result = new BattleCirclePointPackage[amountOfPoints];
            
            BattleCirclePointPackage package = new BattleCirclePointPackage()
            {
                PointInCircle = forward * circleRange,
                AngleInCircle = 0
            };
            result[0] = package;
            
            if (amountOfPoints == 1)
            {
                return result;
            }
            
            float radIncrease = Mathf.Deg2Rad*(360f / amountOfPoints);
            for (int i = 1; i < amountOfPoints; i++)
            {
                Vector2 xzDir = VectorMath.Rotate(new Vector2(forward.x, forward.z).normalized,radIncrease*i);
                Vector3 pointDir = new Vector3(xzDir.x, 0, xzDir.y);

                package.PointInCircle = pointDir * circleRange;
                package.AngleInCircle = Mathf.Rad2Deg*(radIncrease * i);
                result[i] = package;
            }

            return result;
        }
        
        /*
        /// <summary>
        /// Initial enemy base-orientation for first position --> Rotates toward the first enemy
        /// Begins at 0 degrees and continues clockwise (cw) (Left-handed system).
        /// </summary>
        public static Vector3[] CreateAllPoints(BattleCircleData data, uint amountOfPoints, Transform transform)
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
                    result[i] = forward * circleRange;
                }
                
                Vector2 xzDir = VectorMath.Rotate(new Vector2(forward.x, forward.z).normalized,angleIncrease*i);
                Vector3 pointDir = new Vector3(xzDir.x, 0, xzDir.y);
                result[i] = pointDir * circleRange;
            }

            return result;
        }*/
        
        public void AssignAI2Point(BlackboardReference blackboard)
        {
            /*//Uppdatera positionen på transforms
            for (int i = 0; i < _aisAndTargetTransforms.Values.Count; i++)
            {
                Vector3 localPoint = localPoints[i];
                Transform existingPointTransform = _aisAndTargetTransforms.Values.ToArray()[i];
                
                existingPointTransform.position = _battleCircleTransform.position+localPoint;
            }
            
            //Tilldela den nya punkten till den nya fienden
            Transform pointTransform = new GameObject().transform;
            pointTransform.parent = _battleCircleTransform;
            pointTransform.position = _battleCircleTransform.position + localPoints[^1];
                                     
            _aisAndTargetTransforms[blackboard] = pointTransform;
            blackboard.SetVariableValue("InBattleCircle", true);
            SetAITransformPoint(blackboard, pointTransform);*/
        }
        
        public static BattleCirclePointPackage[] FindAllValidCirclePoints(BattleCirclePointPackage[] pointPackages, AngleSpanPackage[] allAngleSpans)
        {
            List<BattleCirclePointPackage> points = pointPackages.ToList();
                
            foreach (var angleSpan in allAngleSpans)
            {
                uint angleStart = angleSpan.angleStart;
                uint angleEnd = angleSpan.angleEnd;
                List<BattleCirclePointPackage> validPoints = new();
                foreach (var validatingPoint in points)
                {
                    if (!AngleMath.IsWithinAngles(validatingPoint.AngleInCircle, angleStart, angleEnd))
                    {
                        validPoints.Add(validatingPoint);
                    }
                }

                points = validPoints;
            }

            return points.ToArray();
        }

        private void FindClosestTransform()
        {
            
        }
        
        private void ResignAI2Point(BlackboardReference blackboard)
        {
            
        }
    }
}