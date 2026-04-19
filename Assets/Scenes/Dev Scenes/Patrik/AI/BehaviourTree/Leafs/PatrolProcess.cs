using Scenes.Dev_Scenes.Patrik.AI;
using UnityEngine;
using UnityEngine.AI;
using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class PatrolProcess : IProcess
    {
        private readonly NavMeshAgent _agent;
        private readonly IPathPoints _pathPoints;

        //Waypoints
        private int _currentPointIndex = 0;
        private bool _isPathDone = true;

        public PatrolProcess(NavMeshAgent agent, IPathPoints pathPoints)
        {
            _agent = agent;
            _pathPoints = pathPoints;
        }
        
        public NodeState Process()
        {
            Vector3 currentWaypoint = _pathPoints.WorldCoordPatrolPoints[_currentPointIndex];
            if (_isPathDone)
            {
                _agent.SetDestination(currentWaypoint);
            }
            
            if (!_agent.pathPending &&_agent.remainingDistance <= _agent.stoppingDistance)
            {
                _currentPointIndex++;
                if (_currentPointIndex == _pathPoints.WorldCoordPatrolPoints.Length)
                {
                    _currentPointIndex = 0;
                }
                _isPathDone = true;
            }
            
            return NodeState.Processing;
        }

        public void Reset()
        {
            _currentPointIndex = 0;
        }
    }
}