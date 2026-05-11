using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using Unity.Behavior;
using UnityEngine;

namespace AI_Experimental.Unity_Behavior.ExternalComponents
{
     //TODO 4 better performance have an global variant of this and send to all enemies rather then doing the same calculation for every enemy
     public class ExternalTargetHandler : NetworkBehaviour
     {
          [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
          private BlackboardReference _blackboard;

          private readonly Dictionary<int, GameObject> _clientsAsTargets = new ();

          public override void OnStartServer()
          {
               base.OnStartServer();
               _blackboard = behaviorGraphAgent.BlackboardReference;
               ServerManager.OnRemoteConnectionState += HandleLeftPlayer;
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
               AddClientAsTarget(ClientManager.Connection.ClientId);
               ClientSetAllTargets();
               
               if (!IsServerInitialized) enabled = false;
               else enabled = true;
          }

          [ServerRpc(RequireOwnership = false)]
          private void AddClientAsTarget(int clientID)
          {
               if (_clientsAsTargets.ContainsKey(clientID))
               {
                    Debug.LogWarning("client already in " + this);
                    return;
               }

               _clientsAsTargets[clientID] = ServerManager.Clients[clientID].FirstObject.gameObject;
          }
          
          private void HandleLeftPlayer(NetworkConnection networkConnection, RemoteConnectionStateArgs remoteConnectionStateArgs)
          {
               if (remoteConnectionStateArgs.ConnectionState == RemoteConnectionState.Stopped)
               {
                    Debug.Log("Removal of target");
                    RemoveClientAsTarget(networkConnection.ClientId);
                    ServerSetAllTargets();
               }
          }
          
          private void RemoveClientAsTarget(int clientID)
          {
               if (!_clientsAsTargets.Remove(clientID, out GameObject clientAsTarget))
               {
                    Debug.LogWarning("client isn't in dictionary" + this);
                    return;
               }

               Destroy(clientAsTarget);
          }
          
          [ServerRpc(RequireOwnership = false)]
          private void ClientSetAllTargets()
          {
               Debug.Log("Current amount of targets: "+_clientsAsTargets.Keys.Count);
               _blackboard.SetVariableValue("AllTargets", _clientsAsTargets.Values.ToList());
          }
          
          private void ServerSetAllTargets()
          {
               Debug.Log("Current amount of targets: "+_clientsAsTargets.Keys.Count);
               _blackboard.SetVariableValue("AllTargets", _clientsAsTargets.Values.ToList());
          }
     }
}
