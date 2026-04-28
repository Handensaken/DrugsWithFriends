using System;
using System.Diagnostics;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
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
    
    [SerializeReference] public BlackboardVariable<EnemyData> enemyData;


    private float _startColliderTime;
    private float _endColliderTime;
    private float _totTime;
    private float _currentTime;
    
    private bool _activeCollider;
    protected override Status OnStart()
    {
        AttackPackage attackPackage = enemyData.Value.attackPackage;
        _totTime = attackPackage.attackAnimation.length;
        _startColliderTime = _totTime * attackPackage.percentageOfStart;
        _endColliderTime = _totTime * (1-attackPackage.percentageOfEnd);
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _currentTime += Time.deltaTime;
        //Collider on
        if (!_activeCollider && _currentTime >= _startColliderTime)
        {
            collider.Value.SetActive(true); 
            _activeCollider = true;
        }
        //collider off
        else if (_activeCollider && _currentTime >= _endColliderTime)
        {
            collider.Value.SetActive(false); 
        }

        //Animation done
        if (_currentTime >=_totTime)
        {
            return Status.Success;
        }
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
        _currentTime = 0;
        _activeCollider = false;
    }
}

public struct ITimeData
{
    
}

