using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Boolean Wait until ", story: "Wait until [variable] is [requestedValue]", category: "Action/Delay", id: "7d4397999dd93b7ce21a94f0280818cd")]
public partial class BooleanWaitUntilAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Variable;
    [SerializeReference] public BlackboardVariable<bool> RequestedValue;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Variable.Value == RequestedValue.Value)
        {
            return Status.Success;
        }
        return Status.Running;
    }
}

