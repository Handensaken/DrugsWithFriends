using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "In FOV", story: "In [eyes] FOV", category: "Conditions/AI/Sight", id: "dedb93296c79a861c12e95c8006e6b07")]
public partial class InFovCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> Eyes;

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
