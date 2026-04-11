using UnityEngine;
using UnityEngine.AI;
using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class PatrolAction : INodeAction
    {
        private readonly NavMeshAgent _agent;
        private readonly Vector3[] _waypoints;

        //Waypoints
        private int _currentPointIndex = 0;
        private bool _isPathDone;

        public PatrolAction(NavMeshAgent agent, Vector3[] waypoints)
        {
            _agent = agent;
            _waypoints = waypoints;
        }
        
        public NodeState Process()
        {
            Vector3 currentWaypoint = _waypoints[_currentPointIndex];
            _agent.SetDestination(currentWaypoint);

            if (_isPathDone && _agent.remainingDistance <= 0.1f)
            {
                _currentPointIndex++;
                if (_currentPointIndex == _waypoints.Length)
                {
                    _currentPointIndex = 0;
                }
                _isPathDone = false;
            }

            if (_agent.pathPending)
            {
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