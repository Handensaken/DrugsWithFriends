using System;
using System.Collections.Generic;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TryGetTarget", story: "Get suitable [target] from [allTargets]", category: "Action/Interaction",
    id: "00d6238e85e494466c19dbe18182faaa")]
public partial class TryGetTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<List<GameObject>> AllTargets;

    [SerializeReference] public BlackboardVariable<GameObject> self;
    [SerializeReference] public EnemyData enemySO;

    private NavMeshAgent _agent;

    protected override Status OnStart()
    {
        _agent = self.Value.GetComponent<NavMeshAgent>();

        if (AllTargets.Value.Count <= 0)
        {
            Debug.Log("No targets found - Start");
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

        float result = 0;
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            
        }

        return result;
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
        List<Tuple<float, Transform>> evaluationValueAndTransform = new List<Tuple<float, Transform>>();

        foreach (var target in AllTargets.Value)
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
}