using System;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    [CreateAssetMenu(menuName = "Health System/HealthSO")]
    public class HealthSO : ScriptableObject
    {
        public UnityAction<int,int> UpdateHealth = delegate(int healthValue, int amountBatches) {}; //TODO maybe combine both
        public UnityAction<int> UpdateBatch = delegate(int amountBatches) {};
        
        [SerializeField, Min(1)] private int healthPerBatch;
        [SerializeField, Range(1,10)] private int maxAmountBatches;
        
        public int HealthPerBatch => healthPerBatch;
        public int MaxAmountBatches => maxAmountBatches;
    }
}
