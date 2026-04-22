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
    private Vector3 _latestTargetPosition;
    
    //todo -
    //Kontrollera om distansen är tillräcklig för fortsatt körning
    //Stoppingdistance
    
    private bool _firstRunFlag = true;
    protected override Status OnStart() //Tested --> Reduced calls
    {
        //Validate
        if (InvalidParameters())
        {
            return Status.Failure;
        }
        
        Initialize();

        if (IsFirstRun())
        {
            return Status.Running;
        }
        if(!IsUpdatingLatestPosition()){
            return Status.Success; //No need to handle the same value again
}
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
        
        //Continue
        _agent.SetDestination(Target.Value.position); 
        return Status.Running;
    }

    //TODO - reset
    protected override void OnEnd()
    {
        _agent.ResetPath();
    }

    private void Initialize() //TODO in patrol
    {
        _agent ??= Self.Value.GetComponent<NavMeshAgent>();
        _agent.speed = dataSO.Value.stateParameters.movementParameters.Speed;
        _agent.stoppingDistance = dataSO.Value.stateParameters.movementParameters.StoppingDistance;
    }
    
    private bool IsFirstRun()
    {
        if (_firstRunFlag) //otherwise v(0,0,0)
        {
            _latestTargetPosition = Target.Value.transform.position;
            _firstRunFlag = false;
            return true;
        }
        return false;
    }
    
    private bool IsUpdatingLatestPosition()
    {
        Vector3 targetPosition = Target.Value.position;
        if (!IsPositionDifferent(_latestTargetPosition, targetPosition)) return false;
        _latestTargetPosition = targetPosition;
        return true;
    }
    
    private bool IsPositionDifferent(Vector3 a, Vector3 b)
    {
        return !Mathf.Approximately(a.x, b.x) || !Mathf.Approximately(a.y, b.y) || !Mathf.Approximately(a.z, b.z);
    }

    private bool CloseEnough()
    {
        //Vector3 dirToTarget = (_latestTargetPosition-_agent.transform.position).normalized;
        Debug.Log("Latest: " +_latestTargetPosition);
        Debug.Log("Agent: "+eyes.Value.position);
        Debug.Log("Dis: "+Vector3.Distance(_latestTargetPosition, eyes.Value.position));
        return (Vector3.Distance(_latestTargetPosition, eyes.Value.position) <= 3);
    }
    
    private bool InvalidParameters()
    {
        return (!Self.Value || !Target.Value || !eyes.Value || !dataSO.Value);
    }
}

