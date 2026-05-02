using System;
using JetBrains.Annotations;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "[self] chase [target]", category: "Action/Interaction", id: "65b7e88e62ff10f68259ff83a8253f0c")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    
    [SerializeReference] public BlackboardVariable<EnemyData> dataSO;
    [SerializeReference] public BlackboardVariable<Transform> eyes;

    private NavMeshAgent _agent;
    private bool _latestCheck4CloseEnough = true;
    
    protected override Status OnStart()
    {
        //Validate
        if (InvalidParameters())
        {
            return Status.Failure;
        }
        
        Initialize();
        
        return Status.Running;
    }
    
    protected override Status OnUpdate() //TODO
    {
        //Validate
        if (InvalidParameters())
        {
            return Status.Failure;
        }
        
        if (!_agent.pathPending && CloseEnough())
        {
            return Status.Success;
        }

        HandleTargetPosition();
        return Status.Running;
    }

    //TODO - reset
    protected override void OnEnd()
    {
        if (_agent != null)
        {
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();
        }

        _agent = null;
    }

    private bool CloseEnough()
    {
        if (_latestCheck4CloseEnough) 
        {
            _latestCheck4CloseEnough = false;
            return false;
        }
        
        bool result = _agent.remainingDistance <= dataSO.Value.attackPackage.rangeTolerance;
        _latestCheck4CloseEnough = result;
        Debug.Log("remainingDistance: "+_agent.remainingDistance+" - maxRange: "+dataSO.Value.attackPackage.rangeTolerance);
        return result;
    }

    private void HandleTargetPosition()
    {
        Vector3 dirToTarget = (Target.Value.position-eyes.Value.position);
        dirToTarget.y = 0;
        dirToTarget.Normalize(); 
        
        _agent.transform.forward = dirToTarget;
        
        Vector3 targetPosition = Target.Value.position - dirToTarget * dataSO.Value.attackPackage.minRange;
        
        Debug.Log("targetPos: "+Target.Value.position+" - calculatedPos: "+targetPosition+" - AgentPos: "+eyes.Value.position);
        Vector3.Distance(targetPosition,eyes.Value.position);
        _agent.SetDestination(targetPosition); 
    }
    
    private void Initialize()
    {
        _agent = Self.Value.GetComponent<NavMeshAgent>();
        _agent.speed = dataSO.Value.chasePackage.movementPackage.Speed;
        _agent.stoppingDistance = dataSO.Value.chasePackage.movementPackage.StoppingDistance;
        _agent.angularSpeed = 0;
    }
    
    private bool IsPositionDifferent(Vector3 a, Vector3 b)
    {
        return !Mathf.Approximately(a.x, b.x) || !Mathf.Approximately(a.y, b.y) || !Mathf.Approximately(a.z, b.z);
    }
    
    private bool InvalidParameters()
    {
        return (!Self.Value || !Target.Value || !dataSO.Value || !eyes.Value);
    }
}

