using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    public class Health : NetworkBehaviour
    {
        private readonly SyncVar<int> _currentHealth = new SyncVar<int>();

        [SerializeField, Range(0,250)]private int simulateHealthChange;
        
        private readonly SyncVar<int> _currentBatchAmount = new SyncVar<int>();
        [SerializeField, Range(0,20)]private int simulateBatchChange;

        [SerializeField] private bool simulateChange;

        [Space]
        [SerializeField] private HealthSO healthData;

        private void Awake()
        {
            Debug.Log("Awake");
            _currentHealth.OnChange += HandleHealthChange;
            _currentBatchAmount.OnChange += HandleBatchChange;
        }

        private void Update()
        {
            if (IsServerInitialized && simulateChange)
            {
                Debug.Log("Enter");
                _currentHealth.Value += simulateHealthChange;
                _currentBatchAmount.Value += simulateBatchChange;
                simulateChange = false;
            }
        }

        private void HandleHealthChange(int prev, int next, bool asServer)
        {
            healthData.UpdateHealth(next,_currentBatchAmount.Value);
        }
        
        private void HandleBatchChange(int prev, int next, bool asServer)
        {
            healthData.UpdateBatch(next);
        }
    }
}
