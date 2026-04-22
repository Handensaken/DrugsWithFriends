using System;
using JetBrains.Annotations;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "[self] chase [target]: [data]", category: "Action/Interaction", id: "65b7e88e62ff10f68259ff83a8253f0c")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<ScriptableObject> Data;

    private NavMeshAgent _agent;
    private Vector3 _latestTargetPosition;
    
    //todo -
    //Kontrollera valideteten i olika delar
    //Kontrollera om distansen är tillräcklig för fortsatt körning
    //Stoppingdistance
    
    private bool firstRunFlag = true;
    protected override Status OnStart() //Tested --> Reduced calls
    {
        Debug.Log("Chase Start");
        _agent ??= Self.Value.GetComponent<NavMeshAgent>();

        if(IsFirstRun()) return Status.Running;
        if(!IsUpdatingLatestPosition()) return Status.Success; //No need to handle the same value again
        return Status.Running;
    }
    
    protected override Status OnUpdate() //TODO
    {
        //Stoppingdistance
        
        //SetDestination med olika parametrar //TODO in i patrolering
        
        Debug.Log("Chase Update");
       _agent.SetDestination(Target.Value.position); 
       return Status.Success;
    }

    protected override void OnEnd() {}

    private bool IsFirstRun()
    {
        if (firstRunFlag) //otherwise v(0,0,0)
        {
            _latestTargetPosition = Target.Value.transform.position;
            firstRunFlag = false;
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

    private void ValidateParameters()
    {
        
    }

    private void Reset()
    {
        
    }
}

