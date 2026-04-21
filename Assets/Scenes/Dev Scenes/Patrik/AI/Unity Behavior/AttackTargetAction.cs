using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack Target", story: "Attack [Target]", category: "Action/Interaction", id: "38191b92b81f9f5926eba4942ce86359")]
public partial class AttackTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
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

