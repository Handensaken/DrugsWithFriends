using System;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTree
{
    public class BasicEnemy : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private NavMeshAgent agent;
        private BehaviourTree _tree;

        private void Awake()
        {
            _tree = new BehaviourTree("Tree");
            
            _tree.AddChild(new Leaf("Patrol", new PatrolAction(agent, waypoints)));
        }
        
        void Update()
        {
            _tree.Process();
        }
    }
}
