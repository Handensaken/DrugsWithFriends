using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.States;
using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;
using StateMachineScripts.Animation;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineScripts.States.Patrol
{
    public class PatrolStateFactory : IStateFactory
    {
        private readonly Color stateColor;
        private readonly INetworkAgentBehaviour networkAgentBehaviour;
        private readonly NavMeshAgent agent;
        private readonly Vector3[] patrolPoints;
        private readonly ISightData sightData;
        private readonly IMovementData movementData;
        private readonly Transform sightTransform;
        private readonly IAnimationPlayer animationPlayer;
        
        public PatrolStateFactory(
            Color stateColor,
            INetworkAgentBehaviour networkAgentBehaviour,
            NavMeshAgent agent,
            Vector3[] patrolPoints,
            ISightData sightData,
            IMovementData movementData,
            Transform sightTransform)
        {
            this.stateColor = stateColor;
            this.networkAgentBehaviour = networkAgentBehaviour;
            this.agent = agent;
            this.patrolPoints = patrolPoints;
            this.sightData = sightData;
            this.movementData = movementData;
            this.sightTransform = sightTransform;
        }
        
        public IStateTransitions CreateState()
        {
            IStateVisualizer patrolVisualizer = new SeeingStateVisualizer(sightData,stateColor,sightTransform);
            IState patrolState = new PatrolState(networkAgentBehaviour, agent, patrolVisualizer, patrolPoints,movementData);
            return new StateTransitions(patrolState);
        }
    }
}