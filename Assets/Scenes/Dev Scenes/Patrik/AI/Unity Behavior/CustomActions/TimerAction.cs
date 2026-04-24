using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Timer", story: "Update [time]", category: "Action", id: "3218a1916d1934d170cc1745204b64c7")]
public partial class TimerAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Time;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

