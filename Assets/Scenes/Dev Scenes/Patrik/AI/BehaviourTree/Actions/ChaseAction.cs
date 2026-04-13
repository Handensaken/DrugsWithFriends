using BehaviourTree;
using UnityEngine;

public class ChaseAction : INodeAction
{
    public INode.NodeState Process()
    {
        return INode.NodeState.Failure;
    }

    public void Reset() {}
}
