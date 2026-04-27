using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Unity.Behavior;
using UnityEngine;

namespace AI_Experimental.Unity_Behavior.ExternalComponents
{
    //TODO fix Utility AI
    public class ExternalSight : NetworkBehaviour
    {
        [SerializeField] public Transform eyes;

        [SerializeField] public EnemyData enemyData;
        private BehaviorGraphAgent _behaviorGraphAgent;
        private BlackboardReference _blackboard;

        private SightPackage _sightPackage;

        private void Awake()
        {
            _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            _blackboard = _behaviorGraphAgent.BlackboardReference;

            _sightPackage = enemyData.patrolPackage.sightPackage;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsServerInitialized) enabled = false;
        }

        [Server]
        private void Update() //TODO Behaviour tree can disable this component
        {
            Transform[] all = FindAllTargets();
            Transform[] inSightRange = AllTargetsInRange(all, _sightPackage.FOVRange);
            Transform[] inFOV = AllTargetsInAngle(inSightRange, _sightPackage.FOVAngle);

            if (inFOV.Length > 0)
            {
                _blackboard.SetVariableValue("Target", inFOV[0]);
            }
        }

        [Server]
        private Transform[] FindAllTargets()
        {
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
                if (Vector3.Distance(sample.position, eyes.position) <= range)
                {
                    result.Add(sample);
                }
            }
            return result.ToArray();
        }
        [Server]
        private Transform[] AllTargetsInAngle(Transform[] samples ,float angle)
        {
            List<Transform> result = new List<Transform>();
            foreach (var sample in samples)
            {
                Vector3 samplePos = sample.position;
                Vector3 eyesPos = eyes.position;
                Vector2 dirToSample = new Vector2(samplePos.x-eyesPos.x,samplePos.z-eyesPos.z);

                Vector2 forward = new Vector2(eyes.forward.x,eyes.forward.z);
                
                if (Vector2.Angle(dirToSample, forward) <= angle)
                {
                    result.Add(sample);
                }
            }
            return result.ToArray();
        }
    }
}