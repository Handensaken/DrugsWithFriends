using System;
using System.Collections.Generic;
using Scenes.Dev_Scenes.Patrik.AI.Extra;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol Points", story: "[Self] patrol along [Waypoints]", category: "Action/Navigation", id: "61c37b860a3276e02325e083e03653a3")]
public partial class PatrolPointsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<Vector3>> Waypoints;
    
    [SerializeReference] public BlackboardVariable<EnemyData> dataSO;

    private NavMeshAgent _agent;
    //Waypoints
    private int _currentPointIndex = 0;
    private bool _isPathDone = true;
    
    protected override Status OnStart()
    {
        if(InvalidParameters()) return Status.Failure;
        Initialize();
        
        return Status.Running;
    }
    protected override Status OnUpdate()
    {
        Vector3 currentWaypoint = Waypoints.Value[_currentPointIndex];
        
        if (!_agent.pathPending &&_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _currentPointIndex++;
            if (_currentPointIndex == Waypoints.Value.Count)
            {
                _currentPointIndex = 0;
            }
            _isPathDone = true;
        }
        
        if (_isPathDone)
        {
            _agent.SetDestination(currentWaypoint);
            _isPathDone = false;
        }
        
        return Status.Success;
    }
    protected override void OnEnd() {}
    
    private void Initialize()
    {
        _agent = Self.Value.GetComponent<NavMeshAgent>();
        _agent.speed = dataSO.Value.patrolPackage.movementParameters.Speed;
        _agent.stoppingDistance = dataSO.Value.patrolPackage.movementParameters.StoppingDistance;
        _agent.angularSpeed = 600;
    }
    
    private bool InvalidParameters()
    {
        return (!Self.Value || !dataSO.Value || Waypoints.Value == null);
    }
}

