using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTree
{
    public interface INodeAction
    {
        public Node.NodeState Process();
        public void Reset();
    }

    public class PatrolAction : INodeAction
    {
        private readonly NavMeshAgent _agent;
        private readonly Transform[] _waypoints;

        //Waypoints
        private int _currentPointIndex = 0;
        private bool _isPathDone;

        public PatrolAction(NavMeshAgent agent, Transform[] waypoints)
        {
            _agent = agent;
            _waypoints = waypoints;
        }
        
        public Node.NodeState Process()
        {
            Transform currentWaypoint = _waypoints[_currentPointIndex];
            _agent.SetDestination(currentWaypoint.position);

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
            
            return Node.NodeState.Processing;
        }

        public void Reset()
        {
            _currentPointIndex = 0;
        }
    }
}