using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using Unity.Behavior;
using UnityEngine;

namespace AI_Experimental.Unity_Behavior.ExternalComponents
{
     public class ExternalTargetHandler : NetworkBehaviour
     {
          [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
          private BlackboardReference _blackboard;

          private void Awake()
          {
               _blackboard = behaviorGraphAgent.BlackboardReference;
          }

          public override void OnStartServer()
          {
               base.OnStartServer();
               ServerManager.OnRemoteConnectionState += SetAllTargets;
          }

          public override void OnStartClient() //TODO only server run
          {
               base.OnStartClient();
               if (!IsServerInitialized) enabled = false;
               else enabled = true;
          }
     
          [Server]
          private void SetAllTargets(NetworkConnection networkConnection, RemoteConnectionStateArgs remoteConnectionStateArgs)
          {
               GameObject[] targets = GameObject.FindGameObjectsWithTag("Player"); //TODO May have problems in the future 
               Debug.Log(targets.Length);

               _blackboard.SetVariableValue("AllTargets", targets.ToList());
          }
     
     

     
     }
}
