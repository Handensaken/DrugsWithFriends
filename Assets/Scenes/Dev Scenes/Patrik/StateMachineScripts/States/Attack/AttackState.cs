using System;
using StateMachine.Solid;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.States;
using StateMachineScripts.States;
using UnityEngine;
using UnityEngine.AI;

namespace Paket.StateMachineScripts.States.Attack
{
    public class AttackState : State
    {
        private readonly Transform sightTransform;
        private readonly IAttackData attackData;
        private readonly ISelectTarget selectTarget;
        private readonly IAnimationEvent animationEvent;

        private ulong targetClientID;
        
        public AttackState(INetworkAgentBehaviour networkAgentBehaviour,
            NavMeshAgent agent,
            IStateVisualizer stateVisualizer,
            ISelectTarget selectTarget,
            IAnimationEvent animationEvent) : base(networkAgentBehaviour, agent, stateVisualizer)
        {
            this.selectTarget = selectTarget;
            this.animationEvent = animationEvent;
        }

        public override void Enter()
        {
            base.Enter();
            
            NetworkAgentBehaviour.NetStateType = StateType.Attack;
            Agent.speed = 0;
            
            HandleTarget();
            
            if (animationEvent != null)
            {
                animationEvent.OnAnimationEvent += AttackPlayer;
            }
            
        }

        private void HandleTarget()
        {
            if (!selectTarget.Select(out _)) throw new Exception("Missing inRange target");
        }

        public override void Exit()
        {
            base.Exit();
            
            if (animationEvent != null)
            {
                animationEvent.OnAnimationEvent -= AttackPlayer;
            }
        }
        
        private void AttackPlayer()
        {
            
        }
    }
}