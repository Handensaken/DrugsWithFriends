using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Boolean", story: "[boolean] is [targetValue]", category: "Variable Conditions", id: "142b88b46dbad04ae31aedac7336830f")]
public partial class BooleanCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> Boolean;
    [SerializeReference] public BlackboardVariable<bool> TargetValue;

    public override bool IsTrue()
    {
        return Boolean.Value == TargetValue.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
