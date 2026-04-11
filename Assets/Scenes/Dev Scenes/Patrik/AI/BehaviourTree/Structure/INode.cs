namespace BehaviourTree
{
    public interface INode
    {
        public enum NodeState
        {
            Success,
            Failure,
            Processing
        }

        public abstract void AddChild(INode child);

        public abstract NodeState Process();
    }
}