using UnityEngine;

namespace BehaviourTree
{
    public class Decorator : Node
    {
        public Decorator(string debugMessage) : base(debugMessage) {}

        public override void AddChild(Node child)
        {
            if (Children[0] != null)
            {
                Debug.Log( DebugMessage+": - Removed one child to have another" );
            }
            Children[0] = child;
        }
    }
}