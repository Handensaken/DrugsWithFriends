using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsActive", story: "Is [Variable] [boolean]?", category: "Variable Conditions", id: "f67b977b415772ebc876c3814069957e")]
public partial class IsActiveCondition : Condition
{

    [SerializeReference] public BlackboardVariable<GameObject> Variable;
    [SerializeReference] public BlackboardVariable<bool> Boolean;
    
    public override bool IsTrue()
    {
        if (Variable == null)
        {
            return false;
        }
        
        return Variable.Value.activeSelf == Boolean;
    }
}
