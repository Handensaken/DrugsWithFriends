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
    [SerializeReference] public BlackboardVariable<EnemyData> enemySO;

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

    private float UtilityDistanceValue(Vector3 targetPosition, ValuePackage distanceValuePackage)
    {
        _agent.SetDestination(targetPosition);

        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(targetPosition, path); //TODO can be heavy on performance
        
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            float currentDistance = _agent.remainingDistance;
            float startValue = distanceValuePackage.startValue;
            float endValue = distanceValuePackage.endValue;
            float t = (currentDistance-startValue) / (endValue-startValue);
            float curveValue = distanceValuePackage.curve.Evaluate(t);
            return curveValue * distanceValuePackage.weight;
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
        List<Tuple<float, Transform>> evaluationValueAndTransform = new List<Tuple<float, Transform>>();

        
        UtilityAITarget prioritiesAITarget = enemySO.Value.prioritiesAITarget; 
        foreach (var target in AllTargets.Value)
        {
            float sum = 0;
            sum += UtilityDistanceValue(target.transform.position,prioritiesAITarget.distance);
            sum += UtilityMaxHealthValue();
            sum += UtilityCurrentHealthValue();

            Debug.Log(sum);
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