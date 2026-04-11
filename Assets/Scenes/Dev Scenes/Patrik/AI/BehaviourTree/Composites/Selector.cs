using BehaviourTree;
using UnityEngine;
using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class Selector : Node
    {
        public Selector(string debugMessage) : base(debugMessage) {}

        public override NodeState Process()
        {
            foreach (INode child in _children)
            {
                switch (child.Process())
                {
                    case NodeState.Processing:
                        continue;
                    case NodeState.Success:
                        return NodeState.Success;
                }
            }

            return NodeState.Failure;
        }
    }
}

