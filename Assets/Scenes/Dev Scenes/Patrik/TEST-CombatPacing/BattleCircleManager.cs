using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.TEST_CombatPacing
{
     public class BattleCircleManager : NetworkBehaviour
     {
          [SerializeField] private GameObject battleCirclePreFab;
          private readonly Dictionary<int, GameObject> _clientsBattleCircles = new Dictionary<int, GameObject>();
          public override void OnStartClient()
          {
               base.OnStartClient();
               CreateBattleCircle(ClientManager.Connection.ClientId, ClientManager.Connection.FirstObject.transform);
          }
     
          [ServerRpc(RequireOwnership = false)]
          private void CreateBattleCircle(int clientID, Transform clientTransform)
          {
               GameObject battleCircle = Instantiate(battleCirclePreFab,clientTransform);
               _clientsBattleCircles[clientID] = battleCircle;
          }
     }
}