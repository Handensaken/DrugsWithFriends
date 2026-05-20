using System;
using Scenes.Dev_Scenes.Patrik.TEST_CombatPacing;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "LeaveBattleCircle", story: "[Agent] will leave [battleCircle]", category: "Action", id: "480d3b1dd33317f3a6e546445138a0b7")]
public partial class LeaveBattleCircleAction : Action
{
    
    [SerializeReference] public BlackboardVariable<GameObject> agent;
    [SerializeReference] public BlackboardVariable<BattleCircle> battleCircle;
    protected override Status OnStart()
    {
        if (battleCircle.Value == null)
        {
            Debug.Log("Null when trying to leave battleCircle");
            return Status.Success;
        }

        BlackboardReference blackboard = agent.Value.GetComponent<BehaviorGraphAgent>().BlackboardReference;
        battleCircle.Value.RemoveAI(blackboard);
        return Status.Success;
    }
}

