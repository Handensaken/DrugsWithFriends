using System;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

namespace StateMachine.Scripts.StateMachine.Structure
{
    public class AgentPathPoints : MonoBehaviour
    {
        [SerializeField] private Vector3[] localPatrolPoints;
        private Vector3[] _worldCoordPatrolPoints;

        #region EditorHandlesPatrolPoint
        
        private Vector3 _originPathPosition;
        public Vector3 GetOriginalPos => _originPathPosition;
        public Vector3[] GetLocalPatrolPoints => localPatrolPoints;
        public Vector3[] GetWorldPatrolPoints => _worldCoordPatrolPoints;

        #endregion

        private void Awake()
        {
            //For Visualization
            _originPathPosition = transform.position;
            _worldCoordPatrolPoints = new Vector3[localPatrolPoints.Length];
            UpdateWorldPatrolPoints(_originPathPosition);
            
        }

        public void UpdateWorldPatrolPoints(Vector3 startPos)
        {
            _worldCoordPatrolPoints = new Vector3[localPatrolPoints.Length];
            for (int i = 0; i < localPatrolPoints.Length; i++)
            {
                _worldCoordPatrolPoints[i] = localPatrolPoints[i]+startPos;
            }
        }
        
        public void UpdateLocalPatrolPoints(Vector3 startPos)
        {
            for (int i = 0; i < _worldCoordPatrolPoints.Length; i++)
            {
                localPatrolPoints[i] = _worldCoordPatrolPoints[i]-startPos;
            }
        }
        
        private void OnDrawGizmos()
        {
            VisualizePoints();
        }
        
        private void OnValidate()
        {
            if (localPatrolPoints.Length <= 1)
            {
                throw new Exception($"Missing one or more patrolPoints. Current amount: {localPatrolPoints.Length}");
            }
        }
        
        private void VisualizePoints()
        {
            if (localPatrolPoints.Length <= 1) throw new Exception($"Missing patrolsPoints. Current amount: {localPatrolPoints.Length}");
        
            if (!Application.isPlaying)
            {
                for (int i = 0; i < localPatrolPoints.Length; i++)
                {
                    Gizmos.DrawCube(transform.position+localPatrolPoints[i], new Vector3(.5f,.5f,.5f));
                    Gizmos.DrawLine(transform.position+localPatrolPoints[i], transform.position+localPatrolPoints[(i+1)%localPatrolPoints.Length]);
                }
            }
            else
            {
                for (int i = 0; i < _worldCoordPatrolPoints.Length; i++)
                {
                    Gizmos.DrawCube(_worldCoordPatrolPoints[i], new Vector3(.5f,.5f,.5f));
                    Gizmos.DrawLine(_worldCoordPatrolPoints[i], _worldCoordPatrolPoints[(i+1)%_worldCoordPatrolPoints.Length]);
                }
            }
            
            
        }
    }
}
