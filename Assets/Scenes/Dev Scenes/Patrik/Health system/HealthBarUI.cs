using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    public class HealthBarUI : MonoBehaviour
    {
        
        [SerializeField] private RectTransform healthBarUI;
        [SerializeField] private RectTransform healthUI;
        [SerializeField] private GameObject markerUI;

        [Space]
        [SerializeField] private HealthSO healthData;

        [SerializeField] private int currentHealth; //TODO event är bättre
        private List<RectTransform> _markers = new List<RectTransform>();

        private void Update()
        {
            UpdateHealth();
            UpdateHealthBatches();
        }

        private void UpdateHealth() //TODO med event
        {
            float maxHealth = healthData.MaxHealth;
            
            float maxWidth = healthBarUI.rect.width;
            float per = currentHealth / maxHealth;
            float healthWidth = maxWidth * per;
            healthUI.sizeDelta = new Vector2(healthWidth,healthBarUI.rect.height);
        }

        private void UpdateHealthBatches() //TODO med event
        {
            RemoveAllMarkers();

            int amountOfBatches = healthData.AmountOfBatches;
            
            if (amountOfBatches == 1) return;
            
            float maxWidth = healthBarUI.rect.width;
            float distance = maxWidth / amountOfBatches;

            int amountOfMarkers = amountOfBatches - 1;
            for (int i = 1; i <= amountOfMarkers; i++)
            {
                RectTransform newMarker = Instantiate(markerUI,healthBarUI).GetComponent<RectTransform>();
                newMarker.anchoredPosition3D = new Vector2(distance*i,0);
                _markers.Add(newMarker);
            }
        }

        private void RemoveAllMarkers()
        {
            for (int i = 0; i < _markers.Count; i++)
            {
                Destroy(_markers[^(i+1)].gameObject);
            }

            _markers = new List<RectTransform>();
        }
    }
}
