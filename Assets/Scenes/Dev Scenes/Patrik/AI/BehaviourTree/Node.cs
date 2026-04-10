using System.Collections.Generic;

namespace BehaviourTree
{
    public class Node
    {
        public enum NodeState
        {
            Success,
            Failure,
            Processing
        }
        
        protected readonly List<Node> _children = new List<Node>();
        protected int currentChildIndex = 0; 
        
        private string _debugMessage;

        public Node(string debugMessage)
        {
            _debugMessage = debugMessage;
        }
    
        public void AddChild(Node child)
        {
            _children.Add(child);
        }

        public virtual NodeState Process()
        {
            return _children[currentChildIndex].Process();
        }

        protected void Reset()
        {
            currentChildIndex = 0;
            foreach (Node child in _children)
            {
                child.Reset();
            }
        }
    }
}
