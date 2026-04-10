namespace BehaviourTree
{
    public class Leaf : Node
    {
        private readonly INodeAction _nodeAction;
        
        public Leaf(string debugMessage, INodeAction nodeAction) : base(debugMessage)
        {
            _nodeAction = nodeAction;
        }
        
        public override NodeState Process()
        {
            return _nodeAction.Process();
        }
    }
}