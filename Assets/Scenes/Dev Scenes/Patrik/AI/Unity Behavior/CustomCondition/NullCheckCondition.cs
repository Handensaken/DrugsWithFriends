using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "NullCheck", story: "NullCheck: [variable] : [Inverse]", category: "Variable Conditions", id: "aac9d6a5fbd6644432867ab9b531eeac")]
public partial class NullCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable Variable;
    [SerializeReference, Tooltip("Ordinary: null == false. Ínvert null == true")] public BlackboardVariable<bool> inverse;
    

    public override bool IsTrue()
    {
        return inverse ? !Variable.ObjectValue.Equals(null) : Variable.ObjectValue.Equals(null);
    }
}
