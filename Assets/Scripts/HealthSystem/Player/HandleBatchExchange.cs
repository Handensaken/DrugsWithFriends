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
          public int _recivingID, _givingID;
          
          public void SetUp(int receivingID, int givingID)
          {
               _recivingID = receivingID;
               _givingID = givingID;
          }

          public void GiveBatch()
          {
               HealthManager.Instance.TryGiveBatchAmount(_givingID,_recivingID,1);
          }
     
     }
}
