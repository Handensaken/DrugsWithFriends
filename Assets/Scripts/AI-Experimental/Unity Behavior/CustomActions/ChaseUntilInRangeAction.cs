using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase until in range", story: "", category: "Action/Interaction", id: "6ab1f09f07773dde362999d789dbaf99")]
public partial class ChaseUntilInRangeAction : ChaseAction
{
    protected override bool CloseEnough()
    {
        return base.CloseEnough();
    }
}

