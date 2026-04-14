using System.Collections.Generic;
using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class Node : INode
    {
        protected readonly List<Node> Children = new List<Node>();
        protected int CurrentChildIndex = 0; 
        
        protected readonly string DebugMessage;

        public string GetDebugMessage => DebugMessage;

        public Node(string debugMessage)
        {
            DebugMessage = debugMessage;
        }
    
        public virtual void AddChild(Node child)
        {
            Children.Add(child);
        }

        public virtual NodeState Process()
        {
            return Children[CurrentChildIndex].Process();
        }

        public virtual void Reset()
        {
            CurrentChildIndex = 0;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }
}
