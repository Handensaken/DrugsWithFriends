using System;
using JetBrains.Annotations;
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
    private bool _latestCheck4CloseEnough = false;
    
    //todo -
    //Kontrollera om distansen är tillräcklig för fortsatt körning
    //Stoppingdistance
    
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
        
        if (CloseEnough())
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
            _agent.ResetPath();
        }

        _agent = null;
    }

    private bool CloseEnough() //TODO fix parameter for "number"
    {
        if (_latestCheck4CloseEnough) 
        {
            _latestCheck4CloseEnough = false;
            return false;
        }
        
        bool result = _agent.remainingDistance <= 0.2f;
        _latestCheck4CloseEnough = result;
        return result;
    }

    private void HandleTargetPosition()
    {
        Vector3 dirToTarget = (Target.Value.position-eyes.Value.position);
        dirToTarget.y = 0;
        dirToTarget.Normalize(); 
        
        _agent.transform.forward = dirToTarget;
        
        Vector3 targetPosition = Target.Value.position - dirToTarget * 2f; //TODO variable
        
        _agent.SetDestination(targetPosition); 
    }
    
    private void Initialize()
    {
        _agent = Self.Value.GetComponent<NavMeshAgent>();
        _agent.speed = dataSO.Value.stateParameters.movementParameters.Speed;
        _agent.stoppingDistance = dataSO.Value.stateParameters.movementParameters.StoppingDistance;
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

