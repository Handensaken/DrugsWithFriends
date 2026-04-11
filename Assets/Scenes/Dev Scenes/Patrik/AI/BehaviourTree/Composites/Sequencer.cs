using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class Sequencer : Node
    {
        public Sequencer(string debugMessage) : base(debugMessage) {}

        public override NodeState Process()
        {
            foreach (INode child in _children)
            {
                switch (child.Process())
                {
                    case NodeState.Processing:
                        continue;
                    case NodeState.Failure:
                        return NodeState.Failure;
                }
            }

            return NodeState.Success;
        }
    }
}