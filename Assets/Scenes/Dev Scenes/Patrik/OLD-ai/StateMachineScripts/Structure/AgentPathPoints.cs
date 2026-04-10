using System;
using UnityEngine;

namespace StateMachine.Scripts.StateMachine.Structure
{
    public class AgentPathPoints : MonoBehaviour
    {
        [SerializeField] private Vector3[] patrolPoints;

        #region EditorHandlesPatrolPoint
        
        private Vector3 originPathPosition;
        public Vector3[] GetPatrolPoints => patrolPoints;
        public Vector3 GetOriginPathPosition => originPathPosition;

        #endregion

        private void Awake()
        {
            //For Visualization
            originPathPosition = transform.position;
        }

        private void OnDrawGizmos()
        {
            VisualizePoints();
        }
        
        private void OnValidate()
        {
            if (patrolPoints.Length <= 1)
            {
                throw new Exception($"Missing one or more patrolPoints. Current amount: {patrolPoints.Length}");
            }
        }
        
        private void VisualizePoints()
        {
            if (patrolPoints.Length <= 1) throw new Exception($"Missing patrolsPoints. Current amount: {patrolPoints.Length}");
        
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (!Application.isPlaying)
                {
                    Gizmos.DrawCube(transform.position+patrolPoints[i], new Vector3(.5f,.5f,.5f));
                    Gizmos.DrawLine(transform.position+patrolPoints[i], transform.position+patrolPoints[(i+1)%patrolPoints.Length]);
                }
                else
                {
                    Gizmos.DrawCube(originPathPosition+patrolPoints[i], new Vector3(.5f,.5f,.5f));
                    Gizmos.DrawLine(originPathPosition+patrolPoints[i], originPathPosition+patrolPoints[(i+1)%patrolPoints.Length]);
                }
                
            }
        }
    }
}
