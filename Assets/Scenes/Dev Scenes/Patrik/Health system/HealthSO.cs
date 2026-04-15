using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    [CreateAssetMenu(menuName = "Health System/HealthSO")]
    public class HealthSO : ScriptableObject
    {
        [SerializeField, Min(1)] private int healthPerBatch;
        [SerializeField, Range(1,10)] private int amountOfBatches;

        public bool updateData;
        
        public int HealthPerBatch => healthPerBatch;
        public int AmountOfBatches => amountOfBatches;

        private void OnValidate()
        {
            updateData = true;
        }
    }
}
