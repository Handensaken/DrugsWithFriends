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
    private float _totTime = 2;
    private float _currentTime = 0;
    private bool _activeCollider = false;
    protected override Status OnStart()
    {
        //TODO activate trigger animation
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _currentTime += Time.deltaTime;
        if (!_activeCollider && _currentTime <= _endColliderTime)
        {
            collider.Value.SetActive(true); 
            _activeCollider = true;
        }
        else if (_activeCollider && _currentTime >= _endColliderTime)
        {
            collider.Value.SetActive(false); 
        }
        
        /*
         * Animation done --> return succesfull
         */
        return Status.Running;
    }

    protected override void OnEnd()
    {
        collider.Value.SetActive(false); 
        _currentTime = 0;
    }
}

public struct ITimeData
{
    
}

