using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Failer", story: "Always failure", category: "Flow", id: "0677ff54f2b55d9e1179dbfa0b26967c")]
public partial class FailerModifier : Modifier
{

    /// <inheritdoc cref="OnStart" />
    protected override Status OnStart()
    {
        if (Child == null)
        {
            Debug.Log("Missing children to one FailerModifier");
            return Status.Failure;
        }
        Status childStatus = StartNode(Child);
        return FailIfChildIsComplete(childStatus);
    }

    /// <inheritdoc cref="OnUpdate" />
    protected override Status OnUpdate()
    {
        return FailIfChildIsComplete(Child.CurrentStatus);
    }

    private Status FailIfChildIsComplete(Status childStatus)
    {
        if (childStatus is Status.Success or Status.Failure)
        {
            return Status.Failure;
        }
        return Status.Waiting;
    }
}

