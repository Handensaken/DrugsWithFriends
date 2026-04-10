using StateMachine.Solid.StateMachine;
using UnityEngine;

namespace StateMachineScripts.Structure
{
    public class StateMachineVisualize : IStateMachineVisualize
    {
        private readonly IStateMachine _stateMachine;
        private readonly Vector3[] _patrolPoints;
        private readonly Vector3 _startPosition;
        public StateMachineVisualize(IStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public void UpdateGizmo()
        {
            _stateMachine.GetCurrentState.GetState.UpdateVisualization();
        }

        
    }
}