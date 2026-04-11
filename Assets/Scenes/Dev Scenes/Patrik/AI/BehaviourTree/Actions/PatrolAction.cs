using StateMachine.Scripts.StateMachine.Structure;
using UnityEngine;
using UnityEngine.AI;
using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class PatrolAction : INodeAction
    {
        private readonly NavMeshAgent _agent;
        private readonly AgentPathPoints _pathPoints;

        //Waypoints
        private int _currentPointIndex = 0;
        private bool _isPathDone = true;

        public PatrolAction(NavMeshAgent agent, AgentPathPoints pathPoints)
        {
            _agent = agent;
            _pathPoints = pathPoints;
        }
        
        public NodeState Process()
        {
            Vector3 currentWaypoint = _pathPoints.GetWorldPatrolPoints[_currentPointIndex];
            _agent.SetDestination(currentWaypoint);
            
            if (_agent.remainingDistance <= 0.1f)
            {
                _currentPointIndex++;
                if (_currentPointIndex == _pathPoints.GetWorldPatrolPoints.Length)
                {
                    _currentPointIndex = 0;
                }
                _isPathDone = false;
            }
            
            return NodeState.Processing;
        }

        public void Reset()
        {
            _currentPointIndex = 0;
        }
    }
}