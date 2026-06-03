using System;
using AI_Experimental.Extra;
using JetBrains.Annotations;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using UnityEngine.Serialization;


//TODO remove speed here and only use it in adjustSpeed that handles he speed externaly based on conditions
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase a target", story: "Chase [target]", category: "Action/Interaction", id: "65b7e88e62ff10f68259ff83a8253f0c")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<EnemyData> dataSO;
    [SerializeReference] public BlackboardVariable<Transform> eyes;

    protected NavMeshAgent _agent;
    protected bool _latestCheck4CloseEnough = true;
    
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
    
    protected override Status OnUpdate()
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

        _agent.speed = CalculateSpeedValue();
        HandleTargetPosition();
        return Status.Running;
    }

    //TODO - reset
    protected override void OnEnd()
    {
        if (_agent != null)
        {
            _agent.velocity = Vector3.zero;
            if (_agent.hasPath)
            {
                _agent.ResetPath();
            }
        }

        _agent = null;
    }
    
    private float CalculateSpeedValue()
    {
        float distance = Vector3.Distance(Target.Value.position,Self.Value.transform.position);
        return MapValues.MapValueToCurve(distance, dataSO.Value.chasePackage.speedValuePackage);
    }
        
    
    
    protected virtual bool CloseEnough()
    {
        if (_latestCheck4CloseEnough) 
        {
            _latestCheck4CloseEnough = false;
            return false;
        }
        
        bool result = _agent.remainingDistance <= _agent.stoppingDistance;
        _latestCheck4CloseEnough = result;
        return result;
    }

    private void HandleTargetPosition()
    {
        Vector3 targetPosition = Target.Value.position;
        _agent.SetDestination(targetPosition); 
    }
    
    private void Initialize()
    {
        _agent = Self.Value.GetComponent<NavMeshAgent>();
        _agent.acceleration = dataSO.Value.chasePackage.Acceleration;
        _agent.stoppingDistance = dataSO.Value.chasePackage.StoppingDistance;
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

