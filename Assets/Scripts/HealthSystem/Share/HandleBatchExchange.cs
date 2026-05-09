using FishNet.Object;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace HealthSystem.Share
{
     /// <summary>
     /// Used for the 1+ button in shareScene
     /// </summary>
     public class HandleBatchExchange : MonoBehaviour
     {
          [SerializeField] private HealthBarUI parentHealthBarUI;

          public void TryGiveBatch()
          {
               HealthManager.Instance.UpdateBatchAmount(parentHealthBarUI.ID,1);
          }
     
     }
}
