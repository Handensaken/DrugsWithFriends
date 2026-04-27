using System;
using System.Diagnostics;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Debug = UnityEngine.Debug;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack Target", story: "Attack with [Collider]", category: "Action/Interaction", id: "38191b92b81f9f5926eba4942ce86359")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> collider;
    [SerializeReference] public BlackboardVariable<float> time;
    
    private float _currentTime = 0;
    protected override Status OnStart()
    {
        collider.Value.SetActive(true); 
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime >= time)
        {
            collider.Value.SetActive(false); 
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        _currentTime = 0;
    }
}

