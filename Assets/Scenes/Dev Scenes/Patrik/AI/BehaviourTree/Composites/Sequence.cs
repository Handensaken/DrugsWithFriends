using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class Sequence : Node
    {
        public Sequence(string debugMessage) : base(debugMessage) {}

        public override NodeState Process()
        {
            if (CurrentChildIndex < Children.Count)
            {
                switch (Children[CurrentChildIndex].Process())
                {
                    case NodeState.Processing:
                        return NodeState.Processing;
                    
                    case NodeState.Failure:
                        Reset();
                        return NodeState.Failure;
                    
                    default:
                        CurrentChildIndex++;
                        return CurrentChildIndex == Children.Count ? NodeState.Success : NodeState.Processing;
                }
            }

            Reset();
            return NodeState.Success;
        }
    }
}