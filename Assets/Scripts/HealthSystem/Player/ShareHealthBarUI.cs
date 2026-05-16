using HealthSystem.Share;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class ShareHealthBarUI : MonoBehaviour, IHealthBarUI
    {
        [SerializeField] private HealthBarUI healthBarUI;
        [SerializeField] private HandleBatchExchange batchExchange;
        
        public void UpdateUI(HealthPackage healthPackage)
        {
            healthBarUI.UpdateUI(healthPackage);
        }

        public void SetUpHealthBarPlayer(int healthOwnerID)
        {
            healthBarUI.SetUpHealthBarPlayer(healthOwnerID);
        }

        public void SetUpHealthBarNpc() {}

        public void SetUpBatchExchange(int healthOwnerID, int givingID)
        {
            batchExchange.SetUp(healthOwnerID, givingID);
        }

        public bool HasBatchExchange => true;
        public GameObject GameObject => gameObject;
    }
}