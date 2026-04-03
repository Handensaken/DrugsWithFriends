using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.Animation;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.States;
using StateMachine.Solid.Scripts.Targets;
using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;
using StateMachineScripts.Animation;
using StateMachineScripts.Targets;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineScripts.States.Chase
{
    public class ChaseStateFactory : IStateFactory
    {
        private readonly Color stateColor;
        private readonly INetworkAgentBehaviour networkAgentBehaviour;
        private readonly NavMeshAgent agent;
        private readonly ISightData sightData;
        private readonly IMovementData movementData;
        private readonly Transform sightTransform;
        
        public ChaseStateFactory(
            Color stateColor,
            INetworkAgentBehaviour networkAgentBehaviour,
            NavMeshAgent agent,
            ISightData sightData,
            IMovementData movementData,
            Transform sightTransform)
        {
            this.stateColor = stateColor;
            this.networkAgentBehaviour = networkAgentBehaviour;
            this.agent = agent;
            this.sightData = sightData;
            this.movementData = movementData;
            this.sightTransform = sightTransform;
        }
        
        public IStateTransitions CreateState()
        {
            ICertifiedTargetProvider certifiedTargets = new CertifiedTargetProvider(new InSightTargetFilter(sightTransform, sightData).Filter);
            IStateVisualizer chaseVisualizer = new SeeingStateVisualizer(sightData,stateColor,sightTransform);
            ISelectTarget chaseSightClosestTarget = new SelectClosestInSightTarget(certifiedTargets,sightTransform);
            IState chaseState = new ChaseState(networkAgentBehaviour, agent, chaseVisualizer, chaseSightClosestTarget,movementData);
            return new StateTransitions(chaseState);
        }
    }
}