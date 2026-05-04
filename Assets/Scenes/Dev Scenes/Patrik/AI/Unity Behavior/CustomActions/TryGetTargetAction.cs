using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TryGetTarget", story: "Get suitable [target] from [allTargets]", category: "Action/Interaction", id: "00d6238e85e494466c19dbe18182faaa")]
public partial class TryGetTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<GameObject> AllTargets;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}
/*
using System;
using System.Collections.Generic;
using System.IO;
using FishNet;
using FishNet.Connection;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AcquireTarget", story: "Get New [Target] from [AllTargets]", category: "Action/Interaction", id: "db4d9d13cdf182cf25727bb4e4ccd941")]
public partial class AcquireTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject[]> AllTargets;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    
    [SerializeReference] public BlackboardVariable<GameObject> self;
    
    /*private NavMeshAgent _agent;
    protected override Status OnStart()
    {
        _agent = self.Value.GetComponent<NavMeshAgent>();

        if (allTargets.Value.Length <= 0)
        {
            return Status.Failure;
        }
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Target.Value = EvaluateAll();
        return Status.Success;
    }

    protected override void OnEnd()
    {
        if (_agent != null)
        {
            _agent.ResetPath();
        }
        _agent = null;
    }

    private float UtilityDistanceValue(Vector3 targetPosition)
    {
        _agent.SetDestination(targetPosition);
        
        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(targetPosition, path); //TODO can be heavy on performance

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            Debug.Log(_agent.remainingDistance);
        }
        return 0;
    }  
    private float UtilityMaxHealthValue()
    {
        return 0;
    }  
    private float UtilityCurrentHealthValue()
    {
        return 0;
    }  

    private Transform EvaluateAll()
    {
        List< Tuple<float,Transform>> evaluationValueAndTransform = new List<Tuple<float, Transform>>();

        foreach (var target in allTargets.Value)
        {
            float sum = 0;
            sum += UtilityDistanceValue(target.transform.position);
            sum += UtilityMaxHealthValue();
            sum += UtilityCurrentHealthValue();
            
            evaluationValueAndTransform.Add(new Tuple<float, Transform>(sum, target.transform));
        }

        Tuple<float, Transform> currentBest = null;
        foreach (var valueTransform in evaluationValueAndTransform)
        {
            if (currentBest == null)
            {
                currentBest = valueTransform;
                continue;
            }

            float currentEvaluationValue = currentBest.Item1;
            float nextEvaluationValue = valueTransform.Item1;
            
            if (nextEvaluationValue > currentEvaluationValue)
            {
                currentBest = valueTransform;
                continue;
            }
        }
        
        return currentBest.Item2;
    }
*/



