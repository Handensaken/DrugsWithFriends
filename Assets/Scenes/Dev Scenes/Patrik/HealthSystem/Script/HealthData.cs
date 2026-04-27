using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    [CreateAssetMenu(menuName = "Health System/HealthSO")]
    public class HealthData : ScriptableObject
    {
        public UnityAction<HealthPackage> UpdateHealth = delegate(HealthPackage healthPackage) {};
        
        [SerializeField, Min(1)] private int healthPerBatch;
        [SerializeField, Range(1,10)] private int maxAmountBatches;
        
        public int HealthPerBatch => healthPerBatch;
        public int MaxAmountBatches => maxAmountBatches;
    }
}
