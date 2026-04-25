using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    /// <summary>
    /// TODO Performance
    /// </summary>
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
            Debug.Log("Arrived - H:"+healthPackage.HealthAmount +" - B:"+healthPackage.BatchAmount);
            int currentBatchAmount = healthPackage.BatchAmount;
            
            float maxWidth = healthBarUI.rect.width;
            float gapSpace = maxWidth*0.01f;
            float totalGapSpace = gapSpace*(currentBatchAmount - 1);
            float batchWidth = (maxWidth - totalGapSpace)/currentBatchAmount;
            
            UpdateHealthBatches(currentBatchAmount, batchWidth,gapSpace);
            UpdateHealth(healthPackage, batchWidth);
        }
        
        private void UpdateHealthBatches(int currentBatchAmount,float batchWidth, float gapSpace)
        {
            RemoveAllMarkers();
            
            if (currentBatchAmount > healthData.MaxAmountBatches)
            {
                currentBatchAmount = healthData.MaxAmountBatches;
                Debug.LogWarning("UpdateHealthBatches exceeded the limited-value but was corrected");
            }
            
            if (currentBatchAmount == 1) return;
            
             //TODO include variation in padding
            
            for (int i = 0; i < currentBatchAmount; i++)
            {
                if (!Instantiate(healthBatch,healthBarUI).TryGetComponent<HealthBatch>(out HealthBatch newBatch)) throw new Exception("Missing HealthBatch on prefab for healthBatch");
                newBatch.BatchRect.sizeDelta = new Vector2(batchWidth,healthBarUI.rect.height);

                Vector2 pos = new Vector2(batchWidth * i + gapSpace*i,0);
                newBatch.BatchRect.anchoredPosition3D = pos;
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
        
        private void UpdateHealth(HealthPackage healthPackage, float batchWidth)
        {
            int currentHealth = healthPackage.HealthAmount;
            int currentBatchValue = healthPackage.BatchAmount;
            
            int maxHealth = healthData.HealthPerBatch*currentBatchValue;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
                Debug.LogWarning("UpdateHealth exceeded the limited-value but was corrected to: "+maxHealth);
            }

            int limitIndex4FullHealth = currentHealth / healthData.HealthPerBatch;
            float height = healthBarUI.rect.height;
            for (int i = 0; i < _healthBatches.Count; i++)
            {
                if (i < limitIndex4FullHealth)
                {
                    _healthBatches[i].HealthRect.sizeDelta = new Vector2(batchWidth,height);
                }
                else if (i == limitIndex4FullHealth)
                {
                    float per = (currentHealth%healthData.HealthPerBatch) / (float)healthData.HealthPerBatch;
                    float healthWidth = batchWidth * per;
                    _healthBatches[i].HealthRect.sizeDelta = new Vector2(healthWidth,height);
                }
                else
                {
                    _healthBatches[i].HealthRect.sizeDelta = new Vector2(0,0);
                }
            }
        }
    }
}
