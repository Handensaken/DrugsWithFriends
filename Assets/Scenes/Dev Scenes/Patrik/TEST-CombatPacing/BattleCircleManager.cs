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

          [SerializeField] private BattleCircleData battleCircleData;
          
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
               HandleAllOverlappingCircles();
          }

          //TODO prestanda anpassa
          [Server]
          private void HandleAllOverlappingCircles()
          {
               foreach (var mainBattleCircle in _clientsBattleCircles)
               {
                    List<AngleSpanPackage> result = new List<AngleSpanPackage>();
                    foreach (var otherBattleCircle in _clientsBattleCircles)
                    {
                         if (mainBattleCircle.Key == otherBattleCircle.Key)
                         {
                              continue;
                         }

                         Vector3 mainPos = mainBattleCircle.Value.transform.position;
                         Vector3 otherPos = otherBattleCircle.Value.transform.position;
                         float distance = Vector3.Distance(mainPos, otherPos);
                         if (distance > battleCircleData.circleRadius*2)
                         {
                              continue;
                         }

                         Vector3 mainCenter = mainBattleCircle.Value.transform.position;
                         Vector3 otherCenter = otherBattleCircle.Value.transform.position;
                         Vector2 mainCenterXZ = new Vector2(mainCenter.x, mainCenter.z);
                         Vector2 otherCenterXZ = new Vector2(otherCenter.x, otherCenter.z);
                         AngleSpanPackage angleSpanPackage = CalculateIntersectionSpan(
                              mainCenterXZ,
                              otherCenterXZ,
                              battleCircleData.circleRadius);
                         
                         result.Add(angleSpanPackage);
                    }

                    mainBattleCircle.Value.CircleBehaviour.AllCircleOverrides = result.ToArray();
               }
               
          }
          
          private AngleSpanPackage CalculateIntersectionSpan(Vector2 center1, Vector2 center2, float radius)
          {
               Vector2 lineBetweenCenter = center2 - center1;
               float distance = lineBetweenCenter.magnitude;
               
               float distanceToLineCenter = distance / 2;
               float heightFromLineToPoints = Mathf.Sqrt(radius*radius - distanceToLineCenter*distanceToLineCenter);

               Vector2 ccwNormal = new Vector2(-lineBetweenCenter.y,lineBetweenCenter.x).normalized;

               Vector2 startPointLocal = lineBetweenCenter.normalized * distanceToLineCenter +
                                         ccwNormal * heightFromLineToPoints;
               Vector2 endPointLocal = lineBetweenCenter.normalized * distanceToLineCenter +
                                       -ccwNormal * heightFromLineToPoints;
               
               //Clockwise is negative so thats why i use "-" to map it according to more intuitive use
               Vector2 forwardRepresentation = Vector2.up;
               float angleStart = -Vector2.SignedAngle(forwardRepresentation,startPointLocal.normalized); //Is based on local origo
               float angleEnd = -Vector2.SignedAngle(forwardRepresentation,endPointLocal.normalized);
               
               AngleSpanPackage result = new AngleSpanPackage();
               if (angleStart < 0)
               {
                    result.angleStart = (uint)Mathf.RoundToInt(360+angleStart);
               }
               else
               {
                    result.angleStart = (uint)Mathf.RoundToInt(angleStart);
               }
               
               if (angleEnd < 0)
               {
                    result.angleEnd = (uint)Mathf.RoundToInt(360+angleEnd);
                    
               }
               else
               {
                    result.angleEnd = (uint)Mathf.RoundToInt(angleEnd);
               }
               
               return result;
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
          public void RemoveBattleCircle(int clientID)
          {
               Debug.Log("Removal of battleCircle");
               if (!_clientsBattleCircles.Remove(clientID,out BattleCircle battleCircle))
               {
                    Debug.Log("Couldn't find battleCircle");
                    return;
               }
               Destroy(battleCircle.gameObject);
          }
          
          [Server]
          public BattleCircle AssignAI2BattleCircle(int clientID, BlackboardReference blackboard)
          {
               _clientsBattleCircles[clientID].AssignAI(blackboard);
               return _clientsBattleCircles[clientID];
          }
     }
}