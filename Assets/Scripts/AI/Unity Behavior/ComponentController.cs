using FishNet.Object;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.AI.Unity_Behavior
{
     public class ComponentController : NetworkBehaviour
     {
          [SerializeField] private MonoBehaviour[] disableOnClientComponents;

          public override void OnStartClient()
          {
               base.OnStartClient();
               if (!IsServerInitialized)
               {
                    foreach (var component in disableOnClientComponents)
                    {
                         component.enabled = false;
                    }
               }
               else
               {
                    foreach (var component in disableOnClientComponents)
                    {
                         component.enabled = true;
                    }
               }
               
          }
     }
}
