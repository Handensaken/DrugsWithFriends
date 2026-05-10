using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
    public class BattleCircle : MonoBehaviour
    {
        [SerializeField, Range(1,10)] private uint amountOfPositioningPoints;
        [SerializeField,Min(0.1f)] private float circleRange;

        private Vector3[] _pointsInLocalSpace;

        private readonly Dictionary<BlackboardReference, Transform> _aiPointDictionary = new ();

        private void Awake()
        {
            _pointsInLocalSpace = CreateAllPoints();
        }
        
        private void Update()
        {
            UpdateAllAIPosition();
        }

        private void UpdateAllAIPosition()
        {
            int counter = 0;
            foreach (var keyValue in _aiPointDictionary)
            {
                Transform targetTransform = keyValue.Value;
                targetTransform.position = transform.position + _pointsInLocalSpace[counter];
                counter++;
            }
        }
        private void SetAITransformPoint(BlackboardReference blackboard, Transform targetTransform)
        {
            blackboard.SetVariableValue("Target",targetTransform);
        }
        
        /// <summary>
        /// Initial enemy base-orientation for first position --> Rotates toward the first enemy
        /// Begins at 0 degrees and continues clockwise (cw) (Left-handed system).
        /// </summary>
        private Vector3[] CreateAllPoints()
        {
            if (amountOfPositioningPoints == 1)
            {
                return new[] { transform.forward*circleRange};
            }
            
            Vector3[] result = new Vector3[amountOfPositioningPoints];

            Vector3 forward = transform.forward;
            float angleIncrease = Mathf.Deg2Rad*(360f / amountOfPositioningPoints);
            for (int i = 0; i < amountOfPositioningPoints; i++)
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
            if (_aiPointDictionary.Count <= 0)
            {
                Transform pointTransform = new GameObject().transform;
                pointTransform.position = transform.position + _pointsInLocalSpace[0];
                                     
                _aiPointDictionary[blackboard] = pointTransform;
                SetAITransformPoint(blackboard, pointTransform);
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawWireSphere(transform.position, circleRange);
            
            Vector3[] points;
            if (!Application.isPlaying)
            {
                points = CreateAllPoints();
            }
            else
            {
                points = _pointsInLocalSpace;
            }

            foreach (Vector3 point in points)
            {
                Gizmos.DrawSphere(transform.position+point, .2f);
            }
        }
    }
}