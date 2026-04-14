using System;
using FishNet.Object;
using Scenes.Dev_Scenes.Patrik.AI.BehaviourTree.Leafs;
using StateMachine.Scripts.StateMachine.Structure;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTree
{
    public class BasicEnemy : NetworkBehaviour
    {
        private AgentPathPoints _pathPoints;
        private INode _tree;

        [Header("Components:")] [SerializeField]
        private Transform eyes;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private GameObject attack;
        [SerializeField] private Transform attackPoint;
        
        private void Awake()
        {
            if (!TryGetComponent(out AgentPathPoints agentPathPoints))
                throw new Exception("Missing comp - AgentPathPoints");
            _pathPoints = agentPathPoints;
            
            _tree = new Selector("Tree");
            
            //Attack Tree
            Sequence attackSequence = new Sequence("Sequencer for Attack");
            
            Leaf inRangeConditionAttack = new Leaf("In range for attack",new InRangeCondition(eyes,2f)); //TODO fix values
            attackSequence.AddChild(inRangeConditionAttack);

            Leaf attackAction = new Leaf("AttackAction",new AttackProcess(attack, attackPoint));
            attackSequence.AddChild(attackAction);
            
            _tree.AddChild(attackSequence);
            
            //Chase Tree
            //Sequence chaseSequence = new Sequence("Sequencer for chase");

            /*Leaf inRangeConditionChase = new Leaf("in range for chase", new InRangeCondition(eyes, enemyData.stateParameters.sightParameters.Range));
            chaseSequence.AddChild(inRangeConditionChase);
            
            Leaf inFOVConditionChase = new Leaf("in FOV for chase", new FOVCondition(eyes, enemyData.stateParameters.sightParameters.Angle));
            chaseSequence.AddChild(inFOVConditionChase);*/

            Leaf chaseAction = new Leaf("chase action", new ChaseAction(eyes,agent,enemyData.stateParameters.sightParameters.Range, enemyData.stateParameters.sightParameters.Angle));
            _tree.AddChild(chaseAction);
            
            //Patrol
            _tree.AddChild(new Leaf("Patrol", new PatrolProcess(agent, _pathPoints)));
        }

        void Update() //TODO separera visualisering från logik
        {
            if (IsServerInitialized)
            {
                INode.NodeState result = _tree.Process();
                Debug.Log(_tree.GetDebugMessage + " : "+result);
            }
        }
    }
}
