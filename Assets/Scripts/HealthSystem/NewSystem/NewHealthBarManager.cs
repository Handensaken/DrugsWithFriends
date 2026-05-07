using System.Collections.Generic;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class NewHealthBarManager : MonoBehaviour
    {
        [SerializeField] private HealthBarUI playerHealthBarUI;
        
        [Space,SerializeField] private GameObject healthBarForOtherPlayers;
        private readonly Dictionary<int,HealthBarUI> healthBarUis = new Dictionary<int, HealthBarUI>();

        [Space]
        [SerializeField] private HealthRuleData healthRuleData;
        private void OnEnable()
        {
            healthRuleData.UpdateHealth += HandleChanges;
        }

        private void OnDisable()
        {
            healthRuleData.UpdateHealth -= HandleChanges;
        }

        private void HandleChanges(int clientID, HealthPackage healthPackage)
        {
            //Not in the dict --> New Player
            HandleIfNew(clientID);
        }

        private void HandleIfNew(int clientID)
        {
            if (healthBarUis.ContainsKey(clientID))
            {
                return;
            }
            
            CreateBar(clientID);
        }

        private void CreateBar(int clientID)
        {
            
        }
        
        private void CreateBars(int[] barAndClientID)
        {
            int amountOfBars = barAndClientID.Length;
            for (int i = 0; i < amountOfBars; i++)
            {
                Debug.Log("Add: "+i);
                HealthBarUI test = Instantiate(healthBarForOtherPlayers,gameObject.GetComponent<RectTransform>()).GetComponent<HealthBarUI>();
                  
                test.ID = barAndClientID[i];
                //healthBarUis.Add(test);
            }
        }

        private void MoveHealthBars()
        {
            for (int i = 0; i < healthBarUis.Count; i++)
            {
                RectTransform rectTransform = healthBarUis[i].GetComponent<RectTransform>();
                rectTransform.anchoredPosition = playerHealthBarUI.GetComponent<RectTransform>().anchoredPosition + new Vector2(0,40*(i+1));
            }
        }

        private void RemoveHealthBars(int clientID)
        {
            /*for (int i = 0; i < healthBarUis.Count; i++)
            {
                HealthBarUI currentHealthBar = healthBarUis[^(i+1)];
                if (currentHealthBar.ID == clientID)
                {
                    healthBarUis.Remove(currentHealthBar);
                    Destroy(currentHealthBar.gameObject);
                    return;
                }
            }*/
        }
          
        //[ObserversRpc]
        private void HandleAddingBar(int clientID)
        {
            CreateBars(new []{clientID});
            MoveHealthBars();
        }
          
        //[ObserversRpc]
        private void HandleRemovalOfBar(int clientID)
        {
            RemoveHealthBars(clientID);
            MoveHealthBars();
        }
    }
}