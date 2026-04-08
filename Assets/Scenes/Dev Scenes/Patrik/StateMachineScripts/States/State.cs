using StateMachine.Solid.Scripts.Animation;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.States;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineScripts.States
{
    public abstract class State : IState
    {
        protected readonly NavMeshAgent Agent;
        protected readonly INetworkAgentBehaviour NetworkAgentBehaviour;
        private readonly IStateVisualizer stateVisualizer;
        
        protected State(INetworkAgentBehaviour networkAgentBehaviour, NavMeshAgent agent, IStateVisualizer stateVisualizer)
        {
            NetworkAgentBehaviour = networkAgentBehaviour;
            Agent = agent;
            this.stateVisualizer = stateVisualizer;
        }

        public virtual void Enter()
        {
            Debug.Log($"Enter: {GetType().Name}");
        }

        public virtual void Update()
        {
            Debug.Log($"Update: {GetType().Name}");
        }

        public void UpdateVisualization()
        {
            stateVisualizer.Visualize();
        }

        public virtual void Exit()
        {
            Debug.Log($"Exit: {GetType().Name}");
        }
        
        protected void SetAgentMovementValues(IMovementData movementData)
        {
            Agent.speed = movementData.Speed;
            Agent.angularSpeed = movementData.AngularSpeed;
            Agent.acceleration = movementData.Acceleration;
        }
    }
}