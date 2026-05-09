using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    
    public class HealthBarManager : NetworkBehaviour
    {
        [SerializeField] private HealthBarUI playerHealthBarUI;
        
        [Space,SerializeField] private GameObject healthBarForOtherPlayers;
        [SerializeField] private RectTransform parentOtherPlayersBars;
        private readonly Dictionary<int,HealthBarUI> _healthBarUis = new Dictionary<int, HealthBarUI>();

        [Space, Header("Parameters"), SerializeField]
        private uint distanceBetweenOthers;

        [SerializeField,Tooltip("Now from the top, make it drop, that's some.......ui" +
                                "\n- either the other bars is dropping from the parent or going upwards from the parent")]
        private bool fromTheTop;
        
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
            //Debug.Log("RequestSent - "+ClientManager.Connection.ClientId);
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
                //Debug.Log("MainPlayer - noted");
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
                //Debug.Log("Already in local database");
                return;
            }
            
            //Debug.Log("CreateBarID: "+clientID);
            CreateBar(clientID);
            MoveHealthBars();
        }

        private void CreateBar(int clientID)
        {
            Debug.Log("Created new healthBar in database");
            HealthBarUI bar = Instantiate(healthBarForOtherPlayers,parentOtherPlayersBars).GetComponent<HealthBarUI>();
            bar.SetUp(clientID);
            _healthBarUis[clientID] = bar;
        }
        
        private void RemoveClientBar(int clientID)
        {
            if (!_healthBarUis.Remove(clientID, out HealthBarUI ui))
            {
                Debug.LogError("couldn't remove certain clientHealthBar: "+clientID);
                return;
            }
            
            Destroy(ui.gameObject);
            
            MoveHealthBars();
        }
        
        private void MoveHealthBars()
        {
            if (fromTheTop)
            {
                Vector2 startPos = parentOtherPlayersBars.anchoredPosition +
                                   new Vector2(0, parentOtherPlayersBars.sizeDelta.y);
                
                int counter = 0;
                foreach (var keyValue in _healthBarUis)
                {
                    RectTransform rectTransform = keyValue.Value.GetComponent<RectTransform>();
                    
                    rectTransform.anchoredPosition = startPos + new Vector2(0,-distanceBetweenOthers*counter);
                    counter++;
                }
            }
            else
            {
                int counter = 0;
                foreach (var keyValue in _healthBarUis)
                {
                    RectTransform rectTransform = keyValue.Value.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = parentOtherPlayersBars.anchoredPosition + new Vector2(0,distanceBetweenOthers*counter);
                    counter++;
                }
            }
            
            
        }
    }
}