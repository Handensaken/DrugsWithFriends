using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Paket.GameInterfaces;
using Solid.Scripts.SO;
using StateMachine.Scripts.StateMachine.Structure;
using StateMachine.Solid.Scripts.Animation;
using StateMachine.Solid.Scripts.States;
using StateMachine.Solid.StateMachine;
using StateMachineScripts;
using StateMachineScripts.Animation;
using StateMachineScripts.States;
using StateMachineScripts.Structure;
using UnityEngine;
using UnityEngine.AI;

namespace Paket.StateMachineScripts.Structure
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentBehaviour : NetworkBehaviour, INetworkAgentBehaviour, IDamageable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private AgentPathPoints agentPathPoints;
        [SerializeField] private AgentData agentData;
        [SerializeField] private AttackAnimationEventHandler attackAnimationEvent;
        [SerializeField] private AnimationData animationData;
        
        [Space,Header("State-Machine")] 
        [SerializeField] private Transform sightTransform;

        private IStateMachineBuilder stateMachineBuilder;
        private IStateMachine stateMachine;
        private IStateTransitionFactory stateTransitionFactory;
        
        [Space, Header("State-Machine Visualizer")]
        private IStateMachineVisualize stateMachineVisualize;
        
        private IAnimationPlayer animationPlayer;

        //Mostly to include NetworkVariables. could use only NetworkAnimations for syncing.
        private readonly SyncVar<StateType> netStateType = new SyncVar<StateType>();

        #region Properties

        public StateType NetStateType
        {
            get => netStateType.Value; 
            set => netStateType.Value = value;
        }
        
        #endregion
        
        private void Awake()
        {
            stateMachineBuilder = new StateMachineBuilder(
                this,
                agentData,
                agent,
                sightTransform,
                agentPathPoints,
                attackAnimationEvent);
            
            stateTransitionFactory = new StateTransitionFactory(
                agentData,
                sightTransform,
                stateMachineBuilder.GetAStateFactory(StateType.Patrol),
                stateMachineBuilder.GetAStateFactory(StateType.Chase),
                stateMachineBuilder.GetAStateFactory(StateType.Attack),
                new TransitionManaging());

            stateTransitionFactory.ApplyStateTransitions();

            stateMachine = new global::StateMachineScripts.Structure.StateMachine();
            
            stateMachineVisualize = new StateMachineVisualize(stateMachine);
            animationPlayer = new AnimationPlayer(animator,animationData);
        }

        public override void OnStartServer()
        {
            stateMachine.SetState(stateTransitionFactory.GetAStateTransitions(StateType.Patrol));
        }

        public override void OnStartClient() //TODO kanske bra med unsub
        {
            animationPlayer.TryPlay(netStateType.Value);
            netStateType.OnChange += (_, stateType, _) => OnAnimationChange(stateType);
        }

        private void OnAnimationChange(StateType stateType)
        {
            animationPlayer.TryPlay(stateType);
        }
        
        private void Update()
        {
            ServerUpdate();
            ClientUpdate();
        }

        private void ServerUpdate()
        {
            if (IsServer)
            {
                stateMachine.Update();
            }
        }

        private void ClientUpdate() {}
        
        private void OnDrawGizmos()
        {
            stateMachineVisualize?.UpdateGizmo();
        }

        private void OnValidate()
        {
            if (animator == null)
            {
                //throw new Exception("Missing an animator");
                Debug.Log("Missing animator");
            }
            if (attackAnimationEvent == null)
            {
                //throw new Exception("Missing the attackAnimationEvent");
                Debug.Log("Missing the attackAnimationEvent");
            }
            if (agent == null)
            {
                throw new Exception("Missing a navMeshAgent");
            }
            if (agentData == null)
            {
                throw new Exception("Missing an agentData");
            }
            if (sightTransform == null)
            {
                throw new Exception("Missing an eye-transform");
            }
            if (agentPathPoints == null)
            {
                throw new Exception("Missing an agentPathPoint ref");
            }
        }

        public void Damage()
        {
            Debug.Log(NetworkObject.name +" --> Took Damage");
        }
    }
}
