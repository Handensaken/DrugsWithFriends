using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "NullCheck", story: "Is [variable] null return [inverse]", category: "Variable Conditions", id: "aac9d6a5fbd6644432867ab9b531eeac")]
public partial class NullCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable Variable;
    [SerializeReference, Tooltip("Is variable ")] public BlackboardVariable<bool> inverse;
    

    public override bool IsTrue()
    {
        bool check = Variable.ObjectValue is null || Variable.ObjectValue.Equals(null);
        return inverse ? check : !check;
    }
}
