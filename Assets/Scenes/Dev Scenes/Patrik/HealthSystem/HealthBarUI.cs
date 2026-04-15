using System;
using System.Collections.Generic;
using Scenes.Dev_Scenes.Patrik.Health_system;
using Unity.VisualScripting;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private RectTransform healthBarUI;
        [SerializeField] private GameObject healthBatch;
        private List<HealthBatch> _healthBatches = new List<HealthBatch>();
        
        [Space]
        [SerializeField] private HealthSO healthData;

        private void OnEnable()
        {
            healthData.UpdateHealth += HandleChanges;
        }

        private void OnDisable()
        {
            healthData.UpdateHealth -= HandleChanges;
        }

        private void HandleChanges(HealthPackage healthPackage)
        {
            UpdateHealthBatches(healthPackage.BatchAmount);
            UpdateHealth();
        }
        
        private void UpdateHealthBatches(int currentBatchAmount)
        {
            RemoveAllMarkers();
            
            if (currentBatchAmount > healthData.MaxAmountBatches)
            {
                currentBatchAmount = healthData.MaxAmountBatches;
                Debug.LogWarning("UpdateHealthBatches exceeded the limited-value but was corrected");
            }
            
            if (currentBatchAmount == 1) return;
            
            float maxWidth = healthBarUI.rect.width;
            float amountOfGaps = currentBatchAmount - 1;
            float batchWidth = (maxWidth - amountOfGaps)/currentBatchAmount; //TODO include variation in padding
            
            for (int i = 0; i < currentBatchAmount; i++)
            {
                if (!Instantiate(healthBatch,healthBarUI).TryGetComponent<HealthBatch>(out HealthBatch newBatch)) throw new Exception("Missing HealthBatch on prefab for healthBatch");
                Debug.Log(batchWidth);
                newBatch.BatchRect.sizeDelta = new Vector2(batchWidth,healthBarUI.rect.height);
                newBatch.BatchRect.anchoredPosition3D = new Vector2(batchWidth*i+i,0);
                _healthBatches.Add(newBatch);
            }
        }

        private void RemoveAllMarkers()
        {
            for (int i = 0; i < _healthBatches.Count; i++)
            {
                Destroy(_healthBatches[^(i+1)].gameObject);
            }

            _healthBatches = new List<HealthBatch>();
        }
        
        private void UpdateHealth(HealthPackage healthPackage)
        {
            int currentHealth = healthPackage.HealthAmount;
            int currentBatchValue = healthPackage.BatchAmount;
            
            int maxHealth = healthData.HealthPerBatch*currentBatchValue;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
                Debug.LogWarning("UpdateHealth exceeded the limited-value but was corrected to: "+maxHealth);
            }
            
            float maxWidth = healthBarUI.rect.width;
            float per = currentHealth / (float)maxHealth;
            float healthWidth = maxWidth * per;
            //healthUI.sizeDelta = new Vector2(healthWidth,healthBarUI.rect.height);
        }
    }
}
