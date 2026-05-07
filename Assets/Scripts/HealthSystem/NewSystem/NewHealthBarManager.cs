using System.Collections.Generic;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using FishNet.Object;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class NewHealthBarManager : NetworkBehaviour
    {
        [SerializeField] private NewHealthBarUI playerHealthBarUI;
        
        [Space,SerializeField] private GameObject healthBarForOtherPlayers;
        private readonly Dictionary<int,NewHealthBarUI> _healthBarUis = new Dictionary<int, NewHealthBarUI>();

        [Space]
        [SerializeField] private HealthRuleData healthRuleData;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsOwner) return;
            
            Debug.Log("OnStartClient");
            healthRuleData.UpdateHealth += HandleChanges;
            healthRuleData.RemovalOfClientData += RemoveLeftClient;
            
            SettingUpPlayerHealthBar(ClientManager.Connection.ClientId);
            healthRuleData.RequestHealth(ClientManager.Connection.ClientId);
            Debug.Log("RequestSent - "+ClientManager.Connection.ClientId);
        }

        public override void OnStopClient()
        {
            healthRuleData.UpdateHealth -= HandleChanges;
            healthRuleData.RemovalOfClientData -= RemoveLeftClient;
            
            base.OnStopClient();
        }

        private void SettingUpPlayerHealthBar(int clientID)
        {
            Debug.Log("MainPlayer - "+ clientID);
            playerHealthBarUI.ID = clientID;
        }
        
        private void HandleChanges(int clientID, HealthPackage healthPackage)
        {
            if (HandleIfMainPlayer(clientID, healthPackage))
            {
                return;
            }
            
            HandleIfNew(clientID);
            _healthBarUis[clientID].UpdateUI(healthPackage);
        }

        private bool HandleIfMainPlayer(int clientID, HealthPackage healthPackage)
        {
            if (playerHealthBarUI.ID == clientID)
            {
                Debug.Log("MainPlayer - noted");
                playerHealthBarUI.UpdateUI(healthPackage);
                return true;
            }
            return false;
        }
        
        private void HandleIfNew(int clientID)
        {
            if (clientID == playerHealthBarUI.ID || _healthBarUis.ContainsKey(clientID))
            {
                Debug.Log("Already in local database");
                return;
            }
            
            CreateBar(clientID);
            MoveHealthBars();
        }

        private void CreateBar(int clientID)
        {
            Debug.Log("Created new healthBar in database");
            NewHealthBarUI newBar = Instantiate(healthBarForOtherPlayers,gameObject.GetComponent<RectTransform>()).GetComponent<NewHealthBarUI>();
            _healthBarUis[clientID] = newBar;
        }
        
        private void RemoveLeftClient(int clientID)
        {
            NewHealthBarUI healthBar = _healthBarUis[clientID];
            _healthBarUis.Remove(clientID);
            Destroy(healthBar.gameObject);
            
            MoveHealthBars();
        }
        
        private void MoveHealthBars()
        {
            int counter = 0;
            foreach (var keyValue in _healthBarUis)
            {
                RectTransform rectTransform = keyValue.Value.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = playerHealthBarUI.GetComponent<RectTransform>().anchoredPosition + new Vector2(0,40*(counter+1));
                counter++;
            }
        }
        
        /*private void CreateBars(int[] barAndClientID)
        {
            int amountOfBars = barAndClientID.Length;
            for (int i = 0; i < amountOfBars; i++)
            {
                Debug.Log("Add: "+i);
                HealthBarUI test = Instantiate(healthBarForOtherPlayers,gameObject.GetComponent<RectTransform>()).GetComponent<HealthBarUI>();
                  
                test.ID = barAndClientID[i];
                //healthBarUis.Add(test);
            }
        }*/
        
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
          
        /*[ObserversRpc]
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
        }*/
    }
}