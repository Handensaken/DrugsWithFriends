using UnityEngine;
using NodeState = BehaviourTree.INode.NodeState;

namespace BehaviourTree
{
    public class Leaf : Node
    {
        private readonly IProcess _process;
        
        public Leaf(string debugMessage, IProcess process) : base(debugMessage)
        {
            _process = process;
        }
        
        public override NodeState Process()
        {
            NodeState result = _process.Process();
            Debug.Log(DebugMessage + " : " + result);
            return result;
        }
    }
}