using System;
using System.Collections;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Timer", story: "Time Before [boolean]", category: "Conditions/Timer", id: "d202e566fad5af96f949a83f16f8c083")]
public partial class TimerCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> Boolean;
    
    [SerializeReference] public BlackboardVariable<float> Time;
    
    private bool active;
    private bool result;
    
    public override bool IsTrue()
    {
        return result;
    }

    public override void OnStart()
    {
        if (active)
        {
            result = false;
        }
        else
        {
            result = true;
               
        }
    }
}
