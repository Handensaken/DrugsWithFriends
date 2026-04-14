using BehaviourTree;
using Paket.StateMachineScripts.Targets;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.BehaviourTree.Leafs
{
    public class InRangeCondition : IProcess
    {
        private readonly Transform _eyes;
        private readonly float _range;

        public InRangeCondition(Transform eyes, float range)
        {
            _eyes = eyes;
            _range = range;
        }

        public INode.NodeState Process() //TODO --> blackboard
        {
            TargetDummy[] t = Object.FindObjectsByType<TargetDummy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
            if (t.Length <= 0) return INode.NodeState.Failure;

            foreach (TargetDummy target in t)
            {
                if (Vector3.Distance(_eyes.position,target.Position) <= _range)
                {
                    return INode.NodeState.Success;
                }
            }
        
            return INode.NodeState.Failure;
        }

        public void Reset() {}
    }
}
