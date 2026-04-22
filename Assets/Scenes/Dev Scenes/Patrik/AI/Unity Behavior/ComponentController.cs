using FishNet.Object;
using Unity.Behavior;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
     public class ComponentController : NetworkBehaviour
     {
          [SerializeField] private MonoBehaviour[] components;

          public override void OnStartClient()
          {
               base.OnStartClient();
               if (!IsServerInitialized)
               {
                    foreach (var component in components)
                    {
                         component.enabled = false;
                    }
               }
               
          }
     }
}
