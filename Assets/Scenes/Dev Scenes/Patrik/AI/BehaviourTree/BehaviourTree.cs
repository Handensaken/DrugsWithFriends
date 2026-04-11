using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class BehaviourTree : Node
    {
        public BehaviourTree(string debugMessage) : base(debugMessage) {}

        public override NodeState Process()
        {
            foreach (INode child in _children)
            {
                NodeState nodeState = child.Process();
                if (nodeState != NodeState.Success)
                {
                    return nodeState;
                }
            }

            return NodeState.Success;
        }
    }
}