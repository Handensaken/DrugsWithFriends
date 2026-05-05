using FishNet.Component.Animating;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NetworkSetTrigger", story: "Set animation [trigger] on [networkAnimator] to [triggerValue]", category: "Action/Animation", id: "c94283d3f5e5fca7b428cab321d34b57")]
public partial class NetworkSetTriggerAction : Action
{
    [SerializeReference] public BlackboardVariable<string> Trigger;
    [SerializeReference] public BlackboardVariable<NetworkAnimator> NetworkAnimator;
    [SerializeReference] public BlackboardVariable<bool> triggerValue;

    protected override Status OnStart()
    {
        if (NetworkAnimator.Value == null)
        {
            LogFailure("No Animator set.");
            return Status.Failure;
        }

        if (triggerValue.Value)
        {
            NetworkAnimator.Value.SetTrigger(Trigger.Value);
        }
        else
        {
            NetworkAnimator.Value.ResetTrigger(Trigger.Value);
        }

        return Status.Success;
    }
}

