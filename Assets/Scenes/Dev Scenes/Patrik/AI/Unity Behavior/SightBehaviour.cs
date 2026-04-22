using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
    public class SightBehaviour : NetworkBehaviour
    {
        /*TODO -
      Endast server
      Alla klienter
      FOV
      attackFOV (range)
      */
        [SerializeField] public SightVisualization visualization;

        [SerializeField] public EnemyData enemyData;
        private BehaviorGraphAgent _behaviorGraphAgent;
        private BlackboardReference _blackboard;

        private void Awake()
        {
            _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            _blackboard = _behaviorGraphAgent.BlackboardReference;
        }

        private void Update()
        {
            if (IsServerInitialized)
            {
                AllTargetsInRange();
            }
        }

        private void AllTargetsInRange()
        {
            NetworkConnection[] targets = ServerManager.Clients.Values.ToArray();

            //float range = _blackboard.GetVariable()
            List<Transform> result = new List<Transform>();
            for (int i = 0; i < targets.Length; i++)
            {
                
            }
            //Transform t = targets[0]?.FirstObject.transform;
            //_blackboard.SetVariableValue("targetPoint", t);
        }

        private void OnDrawGizmos()
        {
            if(visualization.onlySelectedGizmos) return;
            visualization.Visualize();
        }

        private void OnDrawGizmosSelected()
        {
            if(!visualization.onlySelectedGizmos) return;
            visualization.Visualize();
        }
    }
}