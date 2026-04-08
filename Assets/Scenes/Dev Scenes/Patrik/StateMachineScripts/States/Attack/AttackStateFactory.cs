using Paket.StateMachineScripts.States.Attack;
using StateMachine.Solid;
using StateMachine.Solid.ScriptableObjects;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.States;
using StateMachine.Solid.Scripts.States.Attack;
using StateMachine.Solid.Scripts.Targets;
using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;
using StateMachineScripts.Targets;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineScripts.States.Attack
{
    public class AttackStateFactory : IStateFactory
    {
        private readonly Color stateColor;
        private readonly INetworkAgentBehaviour networkAgentBehaviour;
        private readonly NavMeshAgent agent;
        private readonly IAttackData attackData;
        private readonly ISightData sightData;
        private readonly Transform sightTransform;
        private readonly IAnimationEvent animationEvent;
        
        public AttackStateFactory(
            Color stateColor,
            INetworkAgentBehaviour networkAgentBehaviour,
            NavMeshAgent agent,
            IAttackData attackData,
            ISightData sightData,
            Transform sightTransform,
            IAnimationEvent animationEvent)
        {
            this.stateColor = stateColor;
            this.networkAgentBehaviour = networkAgentBehaviour;
            this.agent = agent;
            this.attackData = attackData;
            this.sightData = sightData;
            this.sightTransform = sightTransform;
            this.animationEvent = animationEvent;
        }
        
        public IStateTransitions CreateState()
        {
            ITargetFilter inSightTarget = new InSightTargetFilter(sightTransform, sightData);
            ICertifiedTargetProvider inChaseSightProvider = new CertifiedTargetProvider(inSightTarget.Filter);
            ISelectTarget selectTarget = new SelectClosestInSightTarget(inChaseSightProvider,sightTransform);
            IStateVisualizer attackStateVisualizer = new AttackStateVisualizer(stateColor, sightTransform, attackData);
            IState attackState = new AttackState(networkAgentBehaviour, agent, attackStateVisualizer, selectTarget, animationEvent);
            return new StateTransitions(attackState);
        }
    }
}