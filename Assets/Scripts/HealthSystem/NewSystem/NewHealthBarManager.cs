using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Client;
using FishNet.Managing.Logging;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class NewHealthBarManager : NetworkBehaviour
    {
        [SerializeField] private NewHealthBarUI playerHealthBarUI;
        
        [Space,SerializeField] private GameObject healthBarForOtherPlayers;
        [SerializeField] private RectTransform parentOtherPlayersBars;
        private readonly Dictionary<int,NewHealthBarUI> _healthBarUis = new Dictionary<int, NewHealthBarUI>();

        [Space]
        [SerializeField] private HealthRuleData healthRuleData;

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            //Debug.Log("OnStartClient");
            healthRuleData.UpdateHealth += HandleChanges;
            healthRuleData.RemovalClientData += RemoveClientBar;
            //ServerManager.OnRemoteConnectionState += RemoveClientBar;
            
            SettingUpPlayerHealthBar(ClientManager.Connection.ClientId);
            Debug.Log("RequestSent - "+ClientManager.Connection.ClientId);
            ServerRequestHealth(ClientManager.Connection.ClientId);
            
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            healthRuleData.UpdateHealth -= HandleChanges;
            healthRuleData.RemovalClientData -= RemoveClientBar;
        }

        [ServerRpc(RequireOwnership = false)]
        private void ServerRequestHealth(int clientId)
        {
            healthRuleData.RequestHealth(clientId);
        }
        
        [Client]
        private void SettingUpPlayerHealthBar(int clientID)
        {
            playerHealthBarUI.SetUp(clientID);
        }
        
        private void HandleChanges(int clientID, HealthPackage healthPackage)
        {
            //MainBar
            if (playerHealthBarUI.ID == clientID)
            {
                Debug.Log("MainPlayer - noted");
                playerHealthBarUI.UpdateUI(healthPackage);
                return;
            }
            
            //Other clients bars
            HandleIfNew(clientID);
            _healthBarUis[clientID].UpdateUI(healthPackage);
        }
        
        private void HandleIfNew(int clientID)
        {
            if (_healthBarUis.ContainsKey(clientID))
            {
                Debug.Log("Already in local database");
                return;
            }
            
            Debug.Log("CreateBarID: "+clientID);
            CreateBar(clientID);
            MoveHealthBars();
        }

        private void CreateBar(int clientID)
        {
            Debug.Log("Created new healthBar in database");
            NewHealthBarUI newBar = Instantiate(healthBarForOtherPlayers,gameObject.GetComponent<RectTransform>()).GetComponent<NewHealthBarUI>();
            newBar.SetUp(clientID);
            _healthBarUis[clientID] = newBar;
        }
        
        private void RemoveClientBar(int clientID)
        {
            if (!_healthBarUis.Remove(clientID, out NewHealthBarUI ui))
            {
                Debug.LogError("couldn't remove certain clientHealthBar: "+clientID);
                return;
            }
            
            Destroy(ui.gameObject);
            
            MoveHealthBars();
        }
        
        private void MoveHealthBars()
        {
            int counter = 0;
            Debug.Log("Amount of keys: "+_healthBarUis.Keys.Count);
            foreach (var keyValue in _healthBarUis)
            {
                RectTransform rectTransform = keyValue.Value.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = parentOtherPlayersBars.anchoredPosition + new Vector2(0,20*counter);
                counter++;
            }
        }
    }
}