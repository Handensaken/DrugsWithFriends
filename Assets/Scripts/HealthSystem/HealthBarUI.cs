using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class HealthBarUI : MonoBehaviour , IHealthBarUI
    {
        
        //TODO Separate to another prefab with specified functionality instead
        [SerializeField] private int ownerID;
        [SerializeField] private RectTransform healthBarUI;
        
        [Space,Header("Text")]
        [SerializeField] private TextMeshProUGUI text4Name;
        [SerializeField] private string customName;
        
        [Space,SerializeField] private GameObject healthBatch;
        private List<HealthBatch> _healthBatches = new List<HealthBatch>();
        
        [Space] 
        [SerializeField] private Color deadColor;
        
        [Space]
        [SerializeField] private HealthRuleData healthRuleData;
        
        public int OwnerID => ownerID;
        public bool HasBatchExchange => false;
        public GameObject GameObject => gameObject;
        

        public void SetUpHealthBarPlayer(int healthOwnerID)
        {
            ownerID = healthOwnerID;
            text4Name.text = "Client - " + healthOwnerID;
        }
        
        public void SetUpHealthBarNpc()
        {
            text4Name.text = customName;
        }
        
        public void UpdateUI(HealthPackage healthPackage)
        {
            //Should validate in other spaces
            Debug.Log("Arrived - H:"+healthPackage.HealthAmount +" - B:"+healthPackage.BatchAmount);
            uint currentBatchAmount = healthPackage.BatchAmount;
            
            float maxWidth = healthBarUI.rect.width;
            float gapSpace = maxWidth*0.01f;
            float totalGapSpace = gapSpace*(currentBatchAmount - 1);
            float batchWidth = (maxWidth - totalGapSpace)/currentBatchAmount;
            
            UpdateHealthBatches(currentBatchAmount, batchWidth,gapSpace);
            UpdateHealth(healthPackage, batchWidth);
            
            if (healthPackage.HealthAmount <= 0)
            {
                ChangeToDeadUI();
            }
        }

        private void ChangeToDeadUI()
        {
            Image[] test = GetComponentsInChildren<Image>();
            foreach (var childImage in test)
            {
                Color resultingColor = (deadColor + childImage.color)/2;
                resultingColor.a = 1;
                childImage.color = resultingColor;
            }
        }
        
        private void UpdateHealthBatches(uint currentBatchAmount,float batchWidth, float gapSpace)
        {
            RemoveAllMarkers();
            
            //TODO include variation in padding
            
            for (int i = 0; i < currentBatchAmount; i++)
            {
                if (!Instantiate(healthBatch, healthBarUI).TryGetComponent<HealthBatch>(out HealthBatch newBatch))
                {
                    throw new Exception("Missing HealthBatch on prefab for healthBatch");
                }
                
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
            uint currentHealth = healthPackage.HealthAmount;
            
            uint limitIndex4FullHealth = currentHealth / healthRuleData.HealthPerBatch;
            float height = healthBarUI.rect.height;
            for (int i = 0; i < _healthBatches.Count; i++)
            {
                if (i < limitIndex4FullHealth)
                {
                    _healthBatches[i].HealthRect.sizeDelta = new Vector2(batchWidth,height);
                }
                else if (i == limitIndex4FullHealth)
                {
                    float per = (currentHealth%healthRuleData.HealthPerBatch) / (float)healthRuleData.HealthPerBatch;
                    float healthWidth = batchWidth * per;
                    _healthBatches[i].HealthRect.sizeDelta = new Vector2(healthWidth,height);
                }
                else
                {
                    _healthBatches[i].HealthRect.sizeDelta = new Vector2(0,0);
                }
            }
        }

        #region Extra

        public void SetUpBatchExchange(int healthOwnerID, int givingID) {}

        #endregion
    }
}