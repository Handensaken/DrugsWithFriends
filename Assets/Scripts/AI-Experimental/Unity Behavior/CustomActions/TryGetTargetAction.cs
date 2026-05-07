using System;
using System.Collections.Generic;
using AI_Experimental.Unity_Behavior.CustomActions;
using FishNet.Object;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
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

    //TODO flag som hanterar flera olika utilityAIVal - SENARE
    
    private HealthManager _healthManager;
    private NavMeshAgent _agent;
    
    protected override Status OnStart()
    {
        if (_healthManager == null)
        {
            _healthManager = HealthManager.Instance;
        }
        
        _agent = self.Value.GetComponent<NavMeshAgent>();

        if (AllTargets.Value.Count <= 0)
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

    private Transform EvaluateAll()
    {
        List<Tuple<float, Transform>> evaluationValueAndTransform = new List<Tuple<float, Transform>>();
        CalculateAllTargetsValues(ref evaluationValueAndTransform);
        Transform bestTarget = CompareForBestTarget(evaluationValueAndTransform);
        return bestTarget;
    }
    
    private void CalculateAllTargetsValues(ref List<Tuple<float, Transform>> evaluationValueAndTransform)
    {
        UtilityAITarget prioritiesAITarget = enemySO.Value.prioritiesAITarget; 
        foreach (var target in AllTargets.Value)
        {
            float distanceValue = EvaluateDistance(target.transform.position, prioritiesAITarget.distance);
            
            int clientID = target.GetComponent<NetworkBehaviour>().OwnerId;
            Debug.Log($"Current clientID {clientID}");
            
            HealthPackage currentHealthStatus = _healthManager.ReadClientHealth(clientID);
            Debug.Log($"Current batch: {currentHealthStatus}");

            uint currentAmountBatches = currentHealthStatus.BatchAmount;
            uint maxBatchAmount = _healthManager.MaxBatchAmount; 
            float maxHealthValue = UtilityAIEvaluations.MapValueToCurveCustomMaxValue(currentAmountBatches,maxBatchAmount,prioritiesAITarget.maxHealth);
            
            uint currentHealth = currentHealthStatus.HealthAmount;
            uint maxHealth = currentHealthStatus.BatchAmount * _healthManager.HealthPerBatch;
            float currentHealthValue = UtilityAIEvaluations.MapValueToCurveCustomMaxValue(currentHealth,maxHealth, prioritiesAITarget.health);
            
            float sum = distanceValue+maxHealthValue+currentHealthValue;
            Debug.Log("Distance:"+ distanceValue +
                      "\nMaxHealth:"+ maxHealthValue +
                      "\nCurrentHealth:"+ currentHealthValue +
                      "\nSum: "+sum);
            evaluationValueAndTransform.Add(new Tuple<float, Transform>(sum, target.transform));
        }
    }

    private Transform CompareForBestTarget(List<Tuple<float, Transform>> evaluationValueAndTransform)
    {
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
            }
        }

        return currentBest.Item2;
    }
    
    private float EvaluateDistance(Vector3 targetPosition, ValuePackage package)
    {
        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(targetPosition, path); 
        _agent.path = path;
            
        return UtilityAIEvaluations.MapValueToCurve(_agent.remainingDistance, package);
    }
}