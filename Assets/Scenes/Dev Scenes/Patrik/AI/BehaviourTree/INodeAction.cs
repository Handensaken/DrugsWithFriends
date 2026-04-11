using NodeState = BehaviourTree.INode.NodeState;
namespace BehaviourTree
{
    public interface INodeAction
    {
        public NodeState Process();
        public void Reset();
    }
}