using System;
using System.Collections.Generic;
using Paket.StateMachineScripts.Targets;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Object = UnityEngine.Object;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "InRange", story: "One of [targets] in range of [eyes]", category: "Action", id: "8b84b76acd28affbd17e0a69a2b01cc7")]
    public partial class InRangeAction : Action
    {
        [SerializeReference] public BlackboardVariable<List<GameObject>> Targets; //Make external
        [SerializeReference] public BlackboardVariable<Transform> Eyes;
        [SerializeReference] public BlackboardVariable<float> Range;
        
        protected override Status OnStart()
        {
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            TargetDummy[] t = Object.FindObjectsByType<TargetDummy>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
            if (t.Length <= 0) return Status.Failure;

            foreach (TargetDummy target in t)
            {
                if (Vector3.Distance(Eyes.Value.position,target.Position) <= Range)
                {
                    return Status.Success;
                }
            }
            
            return Status.Failure;
        }

        protected override void OnEnd()
        {
        }
    }
}

