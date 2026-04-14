namespace BehaviourTree
{
    public interface IProcess
    {
        public INode.NodeState Process();
        public void Reset();
    }
}