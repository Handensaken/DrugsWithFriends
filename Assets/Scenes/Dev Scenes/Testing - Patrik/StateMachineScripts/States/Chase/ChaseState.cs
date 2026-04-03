using Paket.StateMachineScripts.Targets;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.States;
using StateMachine.Solid.Scripts.Targets;
using StateMachineScripts.Animation;
using StateMachineScripts.Targets;
using UnityEngine.AI;

namespace StateMachineScripts.States.Chase
{
    public class ChaseState : State
    {
        private readonly ISelectTarget selectTarget;
        private readonly IMovementData movementData;
        
        public ChaseState(INetworkAgentBehaviour networkAgentBehaviour,
            NavMeshAgent agent,
            IStateVisualizer stateVisualizer,
            ISelectTarget selectTarget,
            IMovementData movementData) : base(networkAgentBehaviour, agent, stateVisualizer)
        {
            this.selectTarget = selectTarget;
            this.movementData = movementData;
        }
        
        public override void Enter() 
        {
            base.Enter();
            
            NetworkAgentBehaviour.NetStateType = StateType.Chase;
            SetAgentMovementValues(movementData);
        }
        
        public override void Update()
        {
            base.Update();
            
            if (selectTarget.Select(out IEnemyTarget target))
            {
                Agent.SetDestination(target.Position);
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            Agent.SetDestination(Agent.transform.position);
        }
    }
}