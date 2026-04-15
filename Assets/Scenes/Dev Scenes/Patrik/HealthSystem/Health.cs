using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Scenes.Dev_Scenes.Patrik.HealthSystem;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    public class Health : NetworkBehaviour
    {
        [SerializeField, Range(0,250)]private int simulateHealthChange;
        [SerializeField, Range(1,20)]private int simulateBatchChange;
        private readonly SyncVar<HealthPackage> _currentHealthData = new SyncVar<HealthPackage>();
        
        [SerializeField] private bool simulateChange;

        [Space]
        [SerializeField] private HealthSO healthData;


        private void Awake()
        {
            //_currentHealthData.OnChange += HandleHealthChange;
        }

        private void Update()
        {
            if (IsServerInitialized && simulateChange)
            {
                Debug.Log(simulateHealthChange+ "-"+ simulateBatchChange);
                HealthPackage healthPackage = new HealthPackage(simulateHealthChange, simulateBatchChange);
                //_currentHealthData.Value = healthPackage;
                healthData.UpdateHealth(healthPackage);
                
                simulateChange = false;
            }
        }

        private void HandleHealthChange(HealthPackage prev, HealthPackage next, bool asServer)
        {
            if (asServer)
            {
                Debug.Log("UpdateHealth");
                healthData.UpdateHealth(next);
            }
        }
    }
}
