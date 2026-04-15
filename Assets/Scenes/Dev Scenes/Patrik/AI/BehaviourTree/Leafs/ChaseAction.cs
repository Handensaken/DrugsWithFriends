using BehaviourTree;
using Paket.StateMachineScripts.Targets;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace Scenes.Dev_Scenes.Patrik.AI.BehaviourTree.Leafs
{
    /// <summary>
    /// TEMP
    /// </summary>
    public class ChaseAction : IProcess
    {
        private Transform _eyes;
        private NavMeshAgent _agent;
        private readonly float _range;
        private float _angleMax;
        
        public ChaseAction(Transform eyes, NavMeshAgent agent , float range, float angleMax)
        {
            _eyes = eyes;
            _agent = agent;
            _range = range;
            _angleMax = angleMax;
        }
        
        public INode.NodeState Process() //TODO introducera blackboard och separera till flera conditions
        {
            
            TargetDummy[] t = Object.FindObjectsByType<TargetDummy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
            if (t.Length <= 0) return INode.NodeState.Failure;

            for (int i = 0; i < t.Length; i++)
            {
                if (Vector3.Distance(_eyes.position,t[i].Position) > _range)
                {
                    t[i] = null;
                }
            }
            
            Vector3 forward = _eyes.forward;
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] == null) continue;
                
                Vector3 dirToTarget = (t[i].Position - _eyes.position).normalized;
                float currentAngle = Vector3.Angle(forward, dirToTarget);

                if (currentAngle > _angleMax) t[i] = null;
            }

            foreach (var VARIABLE in t)
            {
                if (VARIABLE != null)
                {
                    _agent.SetDestination(VARIABLE.Position);
                    return INode.NodeState.Success;
                }
            }

            return INode.NodeState.Failure;
        }

        public void Reset() {}
    }
}
