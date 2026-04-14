using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class REMOVEBehaviourTree : Node
    {
        public REMOVEBehaviourTree(string debugMessage) : base(debugMessage) {}

        public override NodeState Process() //TODO handle failure different? --> Reset currentIndex
        {
            while (CurrentChildIndex < Children.Count)
            {
                NodeState nodeState = Children[CurrentChildIndex].Process();
                if (nodeState != NodeState.Success)
                {
                    return nodeState; // Repeat this child until success
                }

                CurrentChildIndex++;
            }

            return NodeState.Success;
        }
    }
}