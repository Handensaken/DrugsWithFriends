using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    interface IHealthBarUI
    {
        //TODO flags but boolean will do for now
        public bool HasBatchExchange { get; }
        public GameObject GameObject { get; }
        
        public void UpdateUI(HealthPackage healthPackage);
        public void SetUpHealthBarPlayer(int healthOwnerID);
        public void SetUpHealthBarNpc();
        public void SetUpBatchExchange(int healthOwnerID, int givingID);

        
    }
}