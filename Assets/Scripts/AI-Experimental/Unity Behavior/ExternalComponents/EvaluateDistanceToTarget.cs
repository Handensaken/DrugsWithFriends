using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Unity.Behavior;
using UnityEngine;

public class EvaluateDistanceToTarget : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent agent;
    [SerializeField] private EnemyData enemyData;

    private BlackboardReference _blackboard;

    private void Awake()
    {
        _blackboard = agent.BlackboardReference;
    }

    private void FixedUpdate()
    {
        if (_blackboard.GetVariableValue("Target", out Transform target))
        {
            if (target == null)
            {
                return;
            }
            
            _blackboard.SetVariableValue("CloseToTarget",Vector3.Distance(target.position, transform.position) <= 3);
        }
    }
}
