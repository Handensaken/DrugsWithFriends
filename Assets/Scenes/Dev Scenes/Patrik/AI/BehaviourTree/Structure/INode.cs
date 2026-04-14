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

        public string GetDebugMessage { get; }
        
        public void AddChild(Node child);
        
        public abstract NodeState Process();
    }
}