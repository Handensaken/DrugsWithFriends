using BehaviourTree;
using Paket.StateMachineScripts.Targets;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.BehaviourTree.Leafs
{
    public class FOVCondition : IProcess
    {
        private Transform _eyes;
        private float _angleMax;

        public FOVCondition(Transform eyes, float angleMax)
        {
            _eyes = eyes;
            _angleMax = angleMax;
        }
        
        public INode.NodeState Process()
        {
            return AnyTargetInFOV() ? INode.NodeState.Success : INode.NodeState.Failure;
        }

        public void Reset() {}

        private bool AnyTargetInFOV() //TODO --> Blackboard
        {
            Vector3 forward = _eyes.forward;
            TargetDummy[] t = Object.FindObjectsByType<TargetDummy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (TargetDummy target in t)
            {
                Vector3 dirToTarget = (target.Position - _eyes.position).normalized;
                float currentAngle = Vector3.Angle(forward, dirToTarget);

                if (currentAngle <= _angleMax) return true;
            }

            return false;
        }
    }
}
