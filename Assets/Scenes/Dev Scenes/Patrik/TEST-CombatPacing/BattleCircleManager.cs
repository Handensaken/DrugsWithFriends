using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
     public class BattleCircleManager : NetworkBehaviour
     {
          [SerializeField] private GameObject battleCirclePreFab;
          private readonly Dictionary<int, BattleCircle> _clientsBattleCircles = new ();

          private static BattleCircleManager _singleton;
          public static BattleCircleManager Instance => _singleton;

          public BattleCircle ClientBattleCircle(int clientID) => _clientsBattleCircles[clientID];
          
          private void Awake()
          {
               if (_singleton != null)
               {
                    Destroy(gameObject);
                    return; 
               }

               _singleton = this;
          }
          
          public override void OnStartServer()
          {
               base.OnStartServer();
               ServerManager.OnRemoteConnectionState += HandlePlayerConnection;
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
               CreateBattleCircle(ClientManager.Connection.ClientId);
          }
     
          [ServerRpc(RequireOwnership = false)]
          private void CreateBattleCircle(int clientID)
          {
               BattleCircle battleCircle = Instantiate(battleCirclePreFab,transform).GetComponent<BattleCircle>();
               battleCircle.gameObject.name = "BattleCircle: " + clientID;
               battleCircle.SetUpBattleCircle(clientID);
               _clientsBattleCircles[clientID] = battleCircle;
          }

          [Server]
          private void HandlePlayerConnection(NetworkConnection networkConnection, RemoteConnectionStateArgs remoteConnectionStateArgs)
          {
               if (remoteConnectionStateArgs.ConnectionState == RemoteConnectionState.Stopped)
               {
                    RemoveBattleCircle(networkConnection.ClientId);
               }
          }
          
          private void FixedUpdate()
          {
               if (!IsServerInitialized)
               {
                    return;
               }
               
               UpdateAllBattleCirclesPositions();
          }

          [Server]
          private void UpdateAllBattleCirclesPositions()
          {
               foreach (var clientBattleCircle in _clientsBattleCircles)
               {
                    int clientID = clientBattleCircle.Key;
                    BattleCircle battleCircle = clientBattleCircle.Value;
                    
                    Vector3 position = ServerManager.Clients[clientID].FirstObject.transform.position;
                    battleCircle.transform.position = position;
               }
          }

          [Server]
          private void RemoveBattleCircle(int clientID)
          {
               Debug.Log("Removal of battleCircle");
               if (!_clientsBattleCircles.Remove(clientID,out BattleCircle battleCircle))
               {
                    Debug.Log("Couldn't find battleCircle");
               }
               Destroy(battleCircle.gameObject);
          }
          
          [Server]
          public void AssignAI2BattleCircle(int clientID, BlackboardReference blackboard)
          {
               _clientsBattleCircles[clientID].AssignAI(blackboard);
          }
     }
}