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
    private Vector3? _latestTargetPosition = null;
    
    //todo -
    //Kontrollera om distansen är tillräcklig för fortsatt körning
    //Stoppingdistance
    
    protected override Status OnStart() //Tested --> Reduced calls
    {
        //Validate
        if (InvalidParameters())
        {
            return Status.Failure;
        }
        
        Initialize();

        if (_latestTargetPosition == null)
        {
            _latestTargetPosition = Target.Value.position;
            return Status.Running;
        }
        if(!IsUpdatingLatestPosition((Vector3)_latestTargetPosition, Target.Value.position)){
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

    private bool CloseEnough() //TODO fix parameter for "number"
    {
        return Vector3.Distance((Vector3)_latestTargetPosition,eyes.Value.position) <= 3; 
    }
    
    private void Initialize() //TODO in patrol
    {
        _agent ??= Self.Value.GetComponent<NavMeshAgent>();
        _agent.speed = dataSO.Value.stateParameters.movementParameters.Speed;
        _agent.stoppingDistance = dataSO.Value.stateParameters.movementParameters.StoppingDistance;
    }
    
    private bool IsUpdatingLatestPosition(Vector3 latestTargetPosition, Vector3 newPosition)
    {
        if (!IsPositionDifferent(latestTargetPosition, newPosition))
        {
            return false;
        }
        _latestTargetPosition = newPosition;
        return true;
    }
    
    private bool IsPositionDifferent(Vector3 a, Vector3 b)
    {
        return !Mathf.Approximately(a.x, b.x) || !Mathf.Approximately(a.y, b.y) || !Mathf.Approximately(a.z, b.z);
    }
    
    private bool InvalidParameters()
    {
        return (!Self.Value || !dataSO.Value);
    }
}

