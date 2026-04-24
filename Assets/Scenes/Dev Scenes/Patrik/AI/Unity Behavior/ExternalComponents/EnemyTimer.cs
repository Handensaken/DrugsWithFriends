using System;
using FishNet.Object;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior.ExternalComponents
{
     public class EnemyTimer : NetworkBehaviour
     {
          [SerializeField] private BlackboardReference blackboard;

          private void Awake()
          {
               blackboard = GetComponent<BehaviorGraphAgent>().BlackboardReference;
               ///blackboard.GetVariable("Token/Attack");
          }

          public override void OnStartClient()
          {
               base.OnStartClient();
               if (!IsServerInitialized) enabled = false;
          }

          public void HandleTimeForAttack()
          {
               Debug.Log("HandleAttack");
          }
          
          
     }
}
