using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands.CheckIn;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using UnityEngine.Serialization;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol Points", story: "[Self] patrol along [Waypoints]", category: "Action/Navigation", id: "61c37b860a3276e02325e083e03653a3")]
public partial class PatrolPointsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> self;
    [SerializeReference] public BlackboardVariable<List<Vector3>> waypoints;

    private NavMeshAgent _navMeshAgent;
    //Waypoints
    private int _currentPointIndex = 0;
    private bool _isPathDone = true;
    
    protected override Status OnStart()
    {
        if(!ValidateOnStart()) return Status.Failure;
        
        return Status.Running;
    }
    protected override Status OnUpdate()
    {
        //Debug.Log("Update trots att onstart fail");
        //Validation --> failure
        
        Vector3 currentWaypoint = waypoints.Value[_currentPointIndex];
        if (_isPathDone)
        {
            _navMeshAgent.SetDestination(currentWaypoint);
        }
            
        if (!_navMeshAgent.pathPending &&_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            _currentPointIndex++;
            if (_currentPointIndex == waypoints.Value.Count)
            {
                _currentPointIndex = 0;
            }
            _isPathDone = true;
        }
        
        return Status.Success;
    }
    protected override void OnEnd() {}
    private bool ValidateOnStart()
    {
        bool result = true;
        if (!self.Value)
        {   
            result = false;
            Debug.Log("Action must have a self");
        }

        if (waypoints.Value.Count <= 0)
        {
            result = false;
            Debug.Log("Action must have at least one waypoint");
        }

        if (!self.Value.TryGetComponent(out _navMeshAgent))
        {
            result = false;
            Debug.Log("NavMeshAgent missing for action");
        }
        
        return result;
    }
}

