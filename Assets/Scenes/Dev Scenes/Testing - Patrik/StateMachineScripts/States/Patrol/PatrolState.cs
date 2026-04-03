using StateMachine.Solid.Scripts.Animation;
using StateMachine.Solid.Scripts.SO;
using StateMachine.Solid.Scripts.States;
using StateMachineScripts.Animation;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachineScripts.States.Patrol
{
    public class PatrolState : State
    {
        private readonly Vector3 originPoint;
        private readonly Vector3[] patrolPoints;
        private readonly IMovementData movementData;
        private readonly Transform sightPoint;
        
        private int patrolPointIndex;
        
        public PatrolState(INetworkAgentBehaviour networkAgentBehaviour,
            NavMeshAgent agent,
            IStateVisualizer stateVisualizer,
            Vector3[] patrolPoints,
            IMovementData movementData) : base(networkAgentBehaviour, agent, stateVisualizer)
        {
            this.patrolPoints = patrolPoints;
            this.originPoint = agent.transform.position;
            this.movementData = movementData;
        }
        
        public override void Enter()
        {
            base.Enter();
            NetworkAgentBehaviour.NetStateType = StateType.Patrol;
            
            SetTargetPoint();
            SetAgentMovementValues(movementData);
        }
        
        public override void Update()
        {
            base.Update();
            CheckSwapPatrolPoint();
            Debug.Log("Updates");
        }
        
        private void SetTargetPoint()
        {
            Agent.SetDestination(patrolPoints[patrolPointIndex%patrolPoints.Length]+originPoint);
        }
        
        private void CheckSwapPatrolPoint()
        {
            if (!Agent.pathPending && Agent.remainingDistance <= 0.01f)
            {
                patrolPointIndex = (patrolPointIndex+1)%patrolPoints.Length;
                SetTargetPoint();
            }
        }
    }
}