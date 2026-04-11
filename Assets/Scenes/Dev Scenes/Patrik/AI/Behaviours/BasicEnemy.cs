using System;
using FishNet.Object;
using StateMachine.Scripts.StateMachine.Structure;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTree
{
    public class BasicEnemy : NetworkBehaviour
    {
        private AgentPathPoints pathPoints;
        [SerializeField] private NavMeshAgent agent;
        private BehaviourTree _tree;

        private void Awake()
        {
            if (!TryGetComponent(out AgentPathPoints agentPathPoints))
                throw new Exception("Missing comp - AgentPathPoints");
            pathPoints = agentPathPoints;
            
            _tree = new BehaviourTree("Tree");
            
            _tree.AddChild(new Leaf("Patrol", new PatrolAction(agent, pathPoints)));
        }

        void Update() //TODO separera visualisering från logik
        {
            if (IsServerInitialized)
            {
                _tree.Process();
            }
            
        }
    }
}
