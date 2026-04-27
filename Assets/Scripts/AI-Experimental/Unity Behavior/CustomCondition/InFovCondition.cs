using System;
using Paket.StateMachineScripts.Targets;
using Unity.Behavior;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "In FOV", story: "In [eyes] [range] and [angle]", category: "Conditions/AI/Sight", id: "dedb93296c79a861c12e95c8006e6b07")]
public partial class InFovCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> Eyes;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<float> Angle;

    public override bool IsTrue()
    {
        Vector3 forward = Eyes.Value.forward;
        TargetDummy[] t = Object.FindObjectsByType<TargetDummy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (TargetDummy target in t)
        {
            Vector3 dirToTarget = (target.Position - Eyes.Value.position).normalized;
            float currentAngle = Vector3.Angle(forward, dirToTarget);

            if (currentAngle <= Angle.Value) return true;
        }

        return false;
    }

    
    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
