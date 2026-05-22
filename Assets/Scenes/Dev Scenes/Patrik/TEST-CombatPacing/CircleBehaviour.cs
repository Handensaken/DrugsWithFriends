using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class CircleBehaviour : BattleCircleAi
    {
        private readonly Transform _battleCircleTransform;
        private readonly BattleCircleData _data;
        
        private readonly Transform[] _allTransformsTargetPoints;
        private readonly List<Transform> _availableTransformPoints;
        
        private readonly Dictionary<BlackboardReference, Transform> _aisAndTakenTransforms;

        private readonly BattleCirclePointPackage[] _allPoints;
        private BattleCirclePointPackage[] _validPoints;
        private AngleSpanPackage[] _allCircleOverrides = Array.Empty<AngleSpanPackage>(); 
        
        public CircleBehaviour(Transform battleCircleTransform,BattleCircleData data,Dictionary<BlackboardReference, Transform> aisAndTakenTransforms)
        {
            //External parameters
            _battleCircleTransform = battleCircleTransform;
            _data = data;
            _aisAndTakenTransforms = aisAndTakenTransforms;
            
            //Creation of points
            uint amountOfPoints = 20; //TODO parameter
            _allPoints = CreateAllPointsPackages(_data,amountOfPoints, _battleCircleTransform);
            _validPoints = _allPoints.ToArray();

            _allTransformsTargetPoints = CreateAllTargetTransforms();
            _availableTransformPoints = _allTransformsTargetPoints.ToList();
        }

        public Dictionary<BlackboardReference, Transform> AisAndTakenTransforms => _aisAndTakenTransforms;
        public Transform[] AllTransforms => _allTransformsTargetPoints;
        //TODO handle change of available transforms
        public List<Transform> AvailableTransforms => _availableTransformPoints;

        public AngleSpanPackage[] AllCircleOverrides
        {
            get => _allCircleOverrides;
            set
            {
                _allCircleOverrides = value;
                HandleChangeOfValidPoints();
            }
        }

        private Transform[] CreateAllTargetTransforms()
        {
            Transform[] result = new Transform[_allPoints.Length];
            for (int i = 0; i < _allPoints.Length; i++)
            {
                Transform pointTransform = new GameObject().transform;
                pointTransform.parent = _battleCircleTransform;

                BattleCirclePointPackage pointPackage = _allPoints[i];
                pointTransform.position = _battleCircleTransform.position + pointPackage.PointInCircle;
                result[i] = pointTransform;
            }

            return result;
        } 
        
        private void HandleChangeOfValidPoints()
        {
            _validPoints = FindOutOfAngleCirclePoints(_allPoints, _allCircleOverrides);
            if (!FindAiWithInvalidTargets(out BlackboardReference[] aiWithoutValidTargets, out Transform[] takenTargets))
            {
                return;
            }

            if (!FindAllAvailablePoints(takenTargets, out BattleCirclePointPackage[] availablePoints))
            {
                //TODO Send out to other circles
                //TODO Just taunt??
                return;
            }

            Transform[] availableTargetTransforms = AvailableTargetTransforms(availablePoints);

            if (!HandleAssigningAvailableTargets(aiWithoutValidTargets, availableTargetTransforms, out BlackboardReference[] restAI))
            {
                Debug.Log("AI that is still not assigned");
            }
            
        }

        private bool HandleAssigningAvailableTargets(BlackboardReference[] aiWithoutValidTargets ,Transform[] availableTargetTransforms, out BlackboardReference[] theRest)
        {
            List<BlackboardReference> rest = new List<BlackboardReference>();
            for (int i = 0; i < aiWithoutValidTargets.Length; i++)
            {
                BlackboardReference currentAI = aiWithoutValidTargets[i];
                
                if (availableTargetTransforms.Length < i+1)
                {
                    Debug.Log("Less availablePoints then there are enemies");
                    rest.Add(currentAI);
                }
                
                //Handles assigning of new transforms
                Transform target = availableTargetTransforms[i];
                AssignAi2Target(currentAI,target);
            }

            theRest = rest.ToArray();
            return theRest.Length == 0;
        }
        
        private void AssignAi2Target(BlackboardReference ai, Transform target)
        {
            _aisAndTakenTransforms[ai] = target;
            ai.SetVariableValue("Target", target);
        }
        
        private Transform[] AvailableTargetTransforms(BattleCirclePointPackage[] availablePoints)
        {
            List<Transform> result = new List<Transform>();
            foreach (var pointPackage in availablePoints)
            {
                foreach (var targetTransform in _allTransformsTargetPoints)
                {
                    Vector3 evaluationPosition = pointPackage.PointInCircle + _battleCircleTransform.position;
                    if (evaluationPosition == targetTransform.position)
                    {
                        result.Add(targetTransform);
                        break;
                    }
                }
            }

            return result.ToArray();
        }
        
        private bool FindAllAvailablePoints(Transform[] takenTransforms,out BattleCirclePointPackage[] availablePoints)
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
            foreach (var aiAndTarget in _aisAndTakenTransforms)
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
        
        /// <summary>
        /// For when ai comes back from attack, and it should still have its own target.
        /// the target shouldn't be lost
        /// </summary>
        /// <param name="ai"></param>
        public void ReassignSameTarget(BlackboardReference ai)
        {
            //TODO check if still valid with _validPoints
            
            Transform takenTransform = _aisAndTakenTransforms[ai];
            ai.SetVariableValue("Target", takenTransform);
        }
        
        /// <summary>
        /// Initial enemy base-orientation for first position --> Rotates toward the first enemy
        /// Begins at 0 degrees and continues clockwise (cw) (Left-handed system).
        /// </summary>
        public static BattleCirclePointPackage[] CreateAllPointsPackages(BattleCircleData data, uint amountOfPoints, Transform transform)
        {
            Vector3 forward = transform.forward;
            float circleRange = data.circleRadius;
            
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
        
        public void AssignAI(BlackboardReference blackboard)
        {
            //TODO hitta tillgänglig punkt till nya fienden
            if (_availableTransformPoints.Count <= 0)
            {
                Debug.Log("No more available targets but is still trying to assign!");
            }

            Transform target = _availableTransformPoints[0];
            _availableTransformPoints.RemoveAt(0);
            
            _aisAndTakenTransforms[blackboard] = target;
            SetAITransformPoint(blackboard, target);
        }
        
        public static BattleCirclePointPackage[] FindOutOfAngleCirclePoints(BattleCirclePointPackage[] sourcePoints, AngleSpanPackage[] allAngleSpans)
        {
            List<BattleCirclePointPackage> points = sourcePoints.ToList();
                
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
        
        public static BattleCirclePointPackage[] FindOutOfAngleCirclePoints(BattleCirclePointPackage[] sourcePoints, AngleSpanPackage[] allAngleSpans, out BattleCirclePointPackage[] invalidPoint)
        {
            List<BattleCirclePointPackage> points = sourcePoints.ToList();
            List<BattleCirclePointPackage> invalidPoints = new List<BattleCirclePointPackage>();
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
                    else
                    {
                        invalidPoints.Add(validatingPoint);
                    }
                }

                points = validPoints;
            }

            invalidPoint = invalidPoints.ToArray();
            return points.ToArray();
        }
        
        public static BattleCirclePointPackage[] FindAllWalkablePoints(Vector3 walkersPoint, Vector3 circleCenter, BattleCirclePointPackage[] sourcePoints, out BattleCirclePointPackage[] invalidPoints)
        {
            List<BattleCirclePointPackage> valid = new List<BattleCirclePointPackage>();
            List<BattleCirclePointPackage> invalid = new List<BattleCirclePointPackage>();
            foreach (BattleCirclePointPackage pointPackage in sourcePoints)
            {
                NavMeshPath path = new NavMeshPath();

                int areaMask = 1 << NavMesh.GetAreaFromName("Walkable");
                bool isOnNavMesh = NavMesh.SamplePosition(circleCenter + pointPackage.PointInCircle, out NavMeshHit hit,
                    .1f, areaMask);
                if (isOnNavMesh && NavMesh.CalculatePath(walkersPoint,circleCenter+pointPackage.PointInCircle, areaMask, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    valid.Add(pointPackage);
                }
                else
                {
                    invalid.Add(pointPackage);
                }
            }

            invalidPoints = invalid.ToArray();
            return valid.ToArray();
        }
        
        public void RemoveAIAndTakenTransform(BlackboardReference blackboard)
        {
            _aisAndTakenTransforms.Remove(blackboard, out Transform targetTransform);
            _availableTransformPoints.Add(targetTransform);
        }
    }
}