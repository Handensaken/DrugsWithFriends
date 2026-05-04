using System.Collections.Generic;
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
               List<GameObject> result = new List<GameObject>();
               foreach (var netConn in ServerManager.Clients.Values)
               {
                    if (netConn.ClientId == networkConnection.ClientId)
                    {
                         continue;
                    }  
                    result.Add(netConn.FirstObject.gameObject);
               }
               Debug.Log("Current amount of targets: "+result.Count);
               _blackboard.SetVariableValue("AllTargets", result);
          }
     
     

     
     }
}
