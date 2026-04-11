using System;
using System.Collections.Generic;
using Solid.Scripts.SO;
using StateMachine.Scripts.StateMachine.Structure;
using StateMachine.Solid;
using StateMachine.Solid.Scripts.Animation;
using StateMachine.Solid.Scripts.StateMachine;
using StateMachine.Solid.Scripts.States;
using StateMachineScripts.Animation;
using StateMachineScripts.States;
using StateMachineScripts.States.Attack;
using StateMachineScripts.States.Chase;
using StateMachineScripts.States.Patrol;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineScripts.Structure
{
    public class StateMachineBuilder : IStateMachineBuilder
    {
        private IStateTransitionFactory stateTransitionFactory;

        private readonly Dictionary<StateType, IStateFactory> stateFactoriesLibrary = new Dictionary<StateType, IStateFactory>();

        public IStateFactory GetAStateFactory(StateType stateType)
        {
            if (!stateFactoriesLibrary.ContainsKey(stateType)) throw new Exception("Missing state in stateFactoryLibrary");
            
            return stateFactoriesLibrary[stateType];
        }

        public StateMachineBuilder(
            INetworkAgentBehaviour networkAgentBehaviour,
            AgentData agentData,
            NavMeshAgent agent, 
            Transform sightTransform, 
            AgentPathPoints agentPathPoints, 
            IAnimationEvent attackAnimationEvent)
        {
            IStateFactory patrolStateFactory = new PatrolStateFactory(
                Color.blue,
                networkAgentBehaviour,
                agent,
                agentPathPoints.LocalPatrolPoints,
                agentData.PatrolSightData,
                agentData.PatrolMovementData,
                sightTransform);

            stateFactoriesLibrary[StateType.Patrol] = patrolStateFactory;
            
            IStateFactory chaseStateFactory = new ChaseStateFactory(
                Color.yellow,
                networkAgentBehaviour,
                agent,
                agentData.ChaseSightData,
                agentData.ChaseMovementData,
                sightTransform);
            
            stateFactoriesLibrary[StateType.Chase] = chaseStateFactory;
            
            IStateFactory attackStateFactory = new AttackStateFactory(
                Color.red,
                networkAgentBehaviour,
                agent,
                agentData.AttackData,
                agentData.ChaseSightData,
                sightTransform,
                attackAnimationEvent);
            
            stateFactoriesLibrary[StateType.Attack] = attackStateFactory;
        }
    }
}