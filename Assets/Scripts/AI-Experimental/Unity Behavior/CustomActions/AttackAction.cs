using System;
using System.Collections.Generic;
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
    [SerializeReference] public BlackboardVariable<List<GameObject>> attackPoints;

    //Markers for when certain event should happen
    private uint _startColliderFrame;
    private uint _endColliderFrame;
    private uint _totalFrameAmount;

    //DeltaTime and to be able to skip certain frames to catch up 
    private float _timePerFrame;
    private float _currentTime; 
    
    private bool _activeCollider;
    protected override Status OnStart()
    {
        AttackPackage attackPackage = enemyData.Value.attackPackage;
        
        _totalFrameAmount = (uint)(attackPackage.attackAnimation.frameRate*attackPackage.attackAnimation.length);
        _startColliderFrame = attackPackage.frameOfStart;
        _endColliderFrame = attackPackage.frameOfEnd;
        
        _timePerFrame = attackPackage.attackAnimation.length / _totalFrameAmount;
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        _currentTime += Time.deltaTime;
        uint currentFrame = TimeToFrameConverter(_currentTime,_timePerFrame);
        
        //Collider on
        if (!_activeCollider && currentFrame >= _startColliderFrame)
        {
            collider.Value.SetActive(true); 
            _activeCollider = true;
        }
        //collider off
        else if (_activeCollider && currentFrame >= _endColliderFrame)
        {
            collider.Value.SetActive(false); 
        }

        if (currentFrame >= _startColliderFrame)
        {
            float t = (float)(currentFrame - _startColliderFrame) / (_endColliderFrame - _startColliderFrame);
            Vector3 newPos = Vector3.Lerp(attackPoints.Value[0].transform.position,attackPoints.Value[1].transform.position,t);
            collider.Value.transform.position = newPos;
        }
        
        //Animation done
        if (currentFrame >=_totalFrameAmount)
        {
            return Status.Success;
        }
        
        return Status.Running;
    }
    
    protected override void OnEnd()
    {
        _startColliderFrame = 0;
        _endColliderFrame = 0;
        _totalFrameAmount = 0;

        _timePerFrame = 0;
        _currentTime = 0;
        _activeCollider = false;
    }

    private uint TimeToFrameConverter(float time, float timePerFrame)
    {
        uint result = (uint)(time / timePerFrame);
        return result;
    }
}

