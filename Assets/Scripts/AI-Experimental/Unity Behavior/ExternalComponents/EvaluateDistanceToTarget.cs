using System;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Scenes.Dev_Scenes.Patrik.TEST_CombatPacing;
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
        if (_blackboard.GetVariableValue("BattleCircle", out BattleCircle battleCircle))
        {
            if (battleCircle == null)
            {
                return;
            }
            
            _blackboard.SetVariableValue("CloseToTarget",Vector3.Distance(battleCircle.transform.position, transform.position) <= 3);
        }
    }
}
