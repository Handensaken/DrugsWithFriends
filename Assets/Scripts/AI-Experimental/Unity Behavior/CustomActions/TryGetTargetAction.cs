using System;
using System.Collections.Generic;
using AI_Experimental.Unity_Behavior.CustomActions;
using FishNet.Object;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using Scenes.Dev_Scenes.Patrik.TEST_CombatPacing;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TryFindBattleCircle", story: "Connect to suitable [battleCircle] from [allTargets]", category: "Action/Interaction",
    id: "00d6238e85e494466c19dbe18182faaa")]
public partial class TryGetTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<BattleCircle> BattleCircle;
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
        
        if (!EvaluateAll(out int? clientID) || clientID == null)
        {
            return Status.Failure;
        }
        
        BlackboardReference blackboard = self.Value.GetComponent<BehaviorGraphAgent>().BlackboardReference;
        BattleCircle.Value = BattleCircleManager.Instance.AssignAI2BattleCircle((int)clientID,blackboard);
        
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

    private bool EvaluateAll(out int? clientID)
    {
        Debug.Log("EvaluateAll");
        List<Tuple<float, Transform>> evaluationValueAndTransform = new List<Tuple<float, Transform>>();
        CalculateAllTargetsValues(evaluationValueAndTransform);

        if (evaluationValueAndTransform.Count <= 0)
        {
            clientID = null;
            return false;
        }
        
        Transform bestTarget = CompareForBestTarget(evaluationValueAndTransform);
        clientID = bestTarget.GetComponent<NetworkBehaviour>().OwnerId;
        return true;
    }
    
    private void CalculateAllTargetsValues(List<Tuple<float, Transform>> evaluationValueAndTransform)
    {
        UtilityAITarget prioritiesAITarget = enemySO.Value.prioritiesAITarget; 
        foreach (var target in AllTargets.Value)
        {
            if (!target)
            {
                Debug.LogWarning("PossibleTarget is null");
                continue;
            }
            
            float distanceValue = EvaluateDistanceValue(target.transform.position, prioritiesAITarget.distance);
            
            int clientID = target.GetComponent<NetworkBehaviour>().OwnerId;
            
            HealthPackage currentHealthStatus = _healthManager.ReadClientHealth(clientID);
            
            float maxHealthValue = EvaluateMaxHealthValue(currentHealthStatus, prioritiesAITarget.maxHealth);

            float currentHealthValue = EvaluateCurrentHealthValue(currentHealthStatus, prioritiesAITarget.health);
            //TODO evaluate from battleCircle-Data

            float targetingValue = EvaluateTargetingValue(clientID, prioritiesAITarget.weightPerEnemyTargeting);
            
            float sum = distanceValue+maxHealthValue+currentHealthValue+targetingValue;
            Debug.Log("\nClientID:"+ clientID + 
                      "\nDistance:"+ distanceValue +
                      "\nMaxHealth:"+ maxHealthValue +
                      "\nCurrentHealth:"+ currentHealthValue +
                      "\nTargetingValue:"+ targetingValue +
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
    
    private float EvaluateDistanceValue(Vector3 targetPosition, ValuePackage package)
    {
        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(targetPosition, path); 
        _agent.path = path;
            
        return UtilityAIEvaluations.MapValueToCurve(_agent.remainingDistance, package);
    }

    private float EvaluateMaxHealthValue(HealthPackage currentHealthStatus, ValuePackageStart valuePackageStart)
    {
        uint currentAmountBatches = currentHealthStatus.BatchAmount;
        uint maxBatchAmount = _healthManager.MaxBatchAmount; 
        return UtilityAIEvaluations.MapValueToCurveCustomMaxValue(currentAmountBatches,maxBatchAmount,valuePackageStart);
    }

    private float EvaluateCurrentHealthValue(HealthPackage currentHealthStatus, ValuePackageStart valuePackageStart)
    {
        uint currentHealth = currentHealthStatus.HealthAmount;
        uint maxHealth = currentHealthStatus.BatchAmount * _healthManager.HealthPerBatch;
        return UtilityAIEvaluations.MapValueToCurveCustomMaxValue(currentHealth,maxHealth, valuePackageStart);
    }

    private float EvaluateTargetingValue(int clientID, float weightPerEnemy)
    {
        BattleCircle battleCircle = BattleCircleManager.Instance.ClientBattleCircle(clientID);
        int amountOfTargetingEnemies = battleCircle.AmountOfEnemiesInCircle;
        return amountOfTargetingEnemies * -weightPerEnemy;
    }
}