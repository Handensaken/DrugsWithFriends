using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.Health_system
{
    public class HealthMarkerUI : MonoBehaviour
    {
        [SerializeField] private RectTransform healthBarUI;
        [SerializeField] private GameObject markerUI;
        private List<RectTransform> _markers = new List<RectTransform>();
        
        [Space]
        [SerializeField] private HealthSO healthData;
        
        private void OnEnable()
        {
            healthData.UpdateBatch += UpdateHealthBatches;
        }

        private void OnDisable()
        {
            healthData.UpdateBatch -= UpdateHealthBatches;
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
            float distance = maxWidth / currentBatchAmount;

            int amountOfMarkers = currentBatchAmount - 1;
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