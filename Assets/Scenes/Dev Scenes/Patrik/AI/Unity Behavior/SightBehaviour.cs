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
        [SerializeField] public Transform eyes;
        [SerializeField] public SightVisualization visualization;

        [SerializeField] public EnemyData enemyData;
        private BehaviorGraphAgent _behaviorGraphAgent;
        private BlackboardReference _blackboard;

        private void Awake()
        {
            _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            _blackboard = _behaviorGraphAgent.BlackboardReference;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsServerInitialized) enabled = false;
        }

        [Server]
        private void Update()
        {
            Transform[] all = FindAllTargets();
            Transform[] inSightRange = AllTargetsInRange(all, 5);

            if (inSightRange.Length > 0)
            {
                _blackboard.SetVariableValue("Aggressive", true);
                _blackboard.SetVariableValue("targetPoint", inSightRange[0]);
                Debug.Log("chase active");
            }
                
            Transform[] inAttackRange = AllTargetsInRange(inSightRange, 2);
            _blackboard.SetVariableValue("Attack", inAttackRange.Length > 0);
        }

        [Server]
        private Transform[] FindAllTargets()
        {
            Debug.Log("FindAllTargets");
            NetworkConnection[] allTargets = ServerManager.Clients.Values.ToArray();
            
            List<Transform> result = new List<Transform>();
            foreach (var target in allTargets)
            {
                if (!target.FirstObject || !target.FirstObject.IsSpawned) continue;
                result.Add(target.FirstObject?.transform);
            }
            
            return result.ToArray();
        }
        
        [Server]
        private Transform[] AllTargetsInRange(Transform[] samples ,float range)
        {
            List<Transform> result = new List<Transform>();
            foreach (var sample in samples)
            {
                if (Vector3.Distance(sample.position, eyes.position) <= range) result.Add(sample);
            }
            
            return result.ToArray();
        }

        [Server]
        private void OnDrawGizmos()
        {
            if(visualization.onlySelectedGizmos) return;
            visualization.Visualize();
        }

        [Server]
        private void OnDrawGizmosSelected()
        {
            if(!visualization.onlySelectedGizmos) return;
            visualization.Visualize();
        }
    }
}