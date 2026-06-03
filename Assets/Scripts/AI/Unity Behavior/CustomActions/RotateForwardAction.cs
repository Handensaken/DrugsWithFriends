using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RotateForward", story: "Rotate [aiTransform] towards [wantedForward]", category: "Action/Transform", id: "c6c55d97fff638c309c3a3cf3e0bd32c")]
public partial class RotateForwardAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> AiTransform;
    [SerializeReference] public BlackboardVariable<Vector3> WantedForward;
    [SerializeReference] public BlackboardVariable<float> Duration;
    
    [CreateProperty] private float m_Progress;
    [CreateProperty] private Vector3 m_StartForward;
    private Vector3 m_EndForward;

    protected override Status OnStart()
    {
        if (AiTransform.Value == null)
        {
            LogFailure("No Forward-transform set.");
            return Status.Failure;
        }
        
        if (Duration.Value <= 0.0f)
        {
            AiTransform.Value.forward = WantedForward;
            return Status.Success;
        }

        m_StartForward = AiTransform.Value.forward;
        m_EndForward = WantedForward;
        m_Progress = 0.0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float normalizedProgress = Mathf.Min(m_Progress / Duration.Value, 1f);
        AiTransform.Value.forward = Vector3.Slerp(m_StartForward, m_EndForward,normalizedProgress);
        m_Progress += Time.deltaTime;

        return normalizedProgress == 1 ? Status.Success : Status.Running;
    }

    protected override void OnDeserialize()
    {
        // Only target to reduce serialization size.
        m_EndForward = WantedForward;
    }
}

