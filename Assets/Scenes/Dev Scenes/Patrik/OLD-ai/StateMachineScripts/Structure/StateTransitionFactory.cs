using System;
using System.Collections.Generic;
using Solid.Scripts.SO;
using StateMachine.Solid.Scripts.StateMachine;
using StateMachine.Solid.Scripts.States;
using StateMachine.Solid.Scripts.Transitions;
using StateMachine.Solid.Transitions;
using StateMachineScripts.States;
using UnityEngine;

namespace StateMachineScripts.Structure
{
    public class StateTransitionFactory : IStateTransitionFactory
    {
        private readonly IStateFactory patrolStateFactory;
        private readonly IStateFactory chaseStateFactory;
        private readonly IStateFactory attackStateFactory;
        
        private readonly ITransitionManaging transitionManaging;
        
        private readonly AgentData agentData;
        private readonly Transform sightTransform;

        public StateTransitionFactory(
            AgentData agentData,
            Transform sightTransform,
            IStateFactory patrolStateFactory,
            IStateFactory chaseStateFactory,
            IStateFactory attackStateFactory,
            ITransitionManaging transitionManaging
        )
        {
            this.patrolStateFactory = patrolStateFactory;
            this.chaseStateFactory = chaseStateFactory;
            this.attackStateFactory = attackStateFactory;
            
            this.transitionManaging = transitionManaging;
            
            this.agentData = agentData;
            this.sightTransform = sightTransform;
        }
        
        private readonly Dictionary<StateType, IStateTransitions> stateTransitionsLibrary =
            new Dictionary<StateType, IStateTransitions>();
        public IStateTransitions GetAStateTransitions(StateType stateType)
        {
            if (!stateTransitionsLibrary.ContainsKey(stateType)) throw new Exception("Missing the requested state");

            return stateTransitionsLibrary[stateType];
        }
        
        public void ApplyStateTransitions()
        {
            IStateTransitions patrolStateTransitions = patrolStateFactory.CreateState();
            IStateTransitions chaseStateTransitions = chaseStateFactory.CreateState();
            IStateTransitions attackStateTransitions = attackStateFactory.CreateState();

            ITransition[] patrolTransitions = new PatrolTransitionFactory(
                agentData.PatrolSightData,
                agentData.AttackData,
                sightTransform,
                chaseStateTransitions,
                attackStateTransitions).CreateTransition();
            
            transitionManaging.ApplyTransitions(patrolStateTransitions,patrolTransitions);
            stateTransitionsLibrary[StateType.Patrol] = patrolStateTransitions;
            
            ITransition[] chaseTransitions = new ChaseTransitionFactory(
                agentData.ChaseSightData,
                agentData.AttackData,
                sightTransform,
                patrolStateTransitions,
                attackStateTransitions).CreateTransition();
            
            transitionManaging.ApplyTransitions(chaseStateTransitions,chaseTransitions);
            stateTransitionsLibrary[StateType.Chase] = chaseStateTransitions;
            
            ITransition[] attackTransitions = new AttackTransitionFactory(
                agentData.ChaseSightData,
                agentData.AttackData,
                sightTransform,
                patrolStateTransitions,
                chaseStateTransitions).CreateTransition();
            
            transitionManaging.ApplyTransitions(attackStateTransitions,attackTransitions);
            stateTransitionsLibrary[StateType.Attack] = attackStateTransitions;
        }
    }
}