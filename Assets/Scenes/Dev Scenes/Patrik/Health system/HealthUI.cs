using System;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private RectTransform healthBarUI;
        [SerializeField] private RectTransform healthUI;
        [SerializeField] private GameObject markerUI;

        [Space]
        [SerializeField] private HealthSO healthData;
        

        private void OnEnable()
        {
            healthData.UpdateHealth += UpdateHealth;
        }

        private void OnDisable()
        {
            healthData.UpdateHealth -= UpdateHealth;
        }

        private void UpdateHealth(int currentHealth, int currentBatchAmount)
        {
            int maxHealth = healthData.HealthPerBatch*currentBatchAmount;
            
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
                Debug.LogWarning("UpdateHealth exceeded the limited-value but was corrected");
            }
            
            float maxWidth = healthBarUI.rect.width;
            float per = currentHealth / (float)maxHealth;
            float healthWidth = maxWidth * per;
            healthUI.sizeDelta = new Vector2(healthWidth,healthBarUI.rect.height);
        }

        
    }
}
