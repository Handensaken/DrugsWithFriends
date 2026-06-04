using HealthSystem.OtherHealth;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Disconnect", story: "Desapwn from [network]", category: "Action", id: "ca32b8dc5cf2a39fc392211504fc3518")]
public partial class DisconnectAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcHealth> Network;

    protected override Status OnStart()
    {
        Network.Value.HandleDestruction();
        return Status.Success;
    }
}

