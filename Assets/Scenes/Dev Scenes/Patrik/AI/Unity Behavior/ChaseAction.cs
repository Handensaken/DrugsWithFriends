using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "[self] chase [target]", category: "Action/Interaction", id: "65b7e88e62ff10f68259ff83a8253f0c")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    private NavMeshAgent agent;
    
    protected override Status OnStart()
    {
        agent = Self.Value.GetComponent<NavMeshAgent>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        

       return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

