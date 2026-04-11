using System.Collections.Generic;
using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class Node : INode
    {
        protected readonly List<INode> _children = new List<INode>();
        protected int currentChildIndex = 0; 
        
        protected string DebugMessage;

        public Node(string debugMessage)
        {
            DebugMessage = debugMessage;
        }
    
        public virtual void AddChild(INode child)
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
