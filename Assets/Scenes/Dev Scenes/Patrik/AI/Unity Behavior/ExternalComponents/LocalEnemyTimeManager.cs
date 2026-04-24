using System;
using FishNet.Object;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior.ExternalComponents
{
     public class LocalEnemyTimeManager : NetworkBehaviour
     {
          public override void OnStartClient()
          {
               base.OnStartClient();
               if (!IsServerInitialized) enabled = false;
          }

          private void Awake()
          {
               throw new NotImplementedException();
          }
     }
}
