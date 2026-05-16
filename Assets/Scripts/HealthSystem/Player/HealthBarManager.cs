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
        private readonly Dictionary<int,IHealthBarUI> _healthBarUis = new Dictionary<int, IHealthBarUI>();

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
            
            healthRuleData.UpdateHealth += HandleChanges;
            healthRuleData.RemovalClientData += RemoveClientBar;
            
            playerHealthBarUI.SetUpHealthBarPlayer(ClientManager.Connection.ClientId);
            
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
        
        private void HandleChanges(int healthOwnerID, HealthPackage healthPackage)
        {
            //MainBar
            if (playerHealthBarUI.OwnerID == healthOwnerID)
            {
                //Debug.Log("MainPlayer - noted");
                playerHealthBarUI.UpdateUI(healthPackage);
                return;
            }
            
            //Other clients bars
            HandleIfNew(healthOwnerID);
            _healthBarUis[healthOwnerID].UpdateUI(healthPackage);
        }
        
        private void HandleIfNew(int healthOwnerID)
        {
            if (_healthBarUis.ContainsKey(healthOwnerID))
            {
                //Debug.Log("Already in local database");
                return;
            }
            
            //Debug.Log("CreateBarID: "+clientID);
            CreateBar(healthOwnerID);
            MoveHealthBars();
        }

        private void CreateBar(int healthOwnerID)
        {
            Debug.Log("Created new healthBar in database: "+healthOwnerID);
            IHealthBarUI healthBar = Instantiate(healthBarForOtherPlayers,parentOtherPlayersBars).GetComponent<IHealthBarUI>();

            healthBar.SetUpHealthBarPlayer(healthOwnerID);
            
            int localPlayerId = ClientManager.Connection.ClientId;
            if (healthBar.HasBatchExchange)
            {
                healthBar.SetUpBatchExchange(healthOwnerID,localPlayerId);
            }
            
            _healthBarUis[healthOwnerID] = healthBar;
        }
        
        private void RemoveClientBar(int healthOwnerID)
        {
            if (!_healthBarUis.Remove(healthOwnerID, out IHealthBarUI ui))
            {
                Debug.LogError("couldn't remove certain clientHealthBar: "+healthOwnerID);
                return;
            }
            
            Destroy(ui.GameObject);
            
            MoveHealthBars();
        }
        
        private void MoveHealthBars()
        {
            if (fromTheTop)
            {
                Vector2 startPos = new Vector2(0, parentOtherPlayersBars.sizeDelta.y);
                
                int counter = 0;
                foreach (var keyValue in _healthBarUis)
                {
                    RectTransform rectTransform = keyValue.Value.GameObject.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = startPos + new Vector2(0,-distanceBetweenOthers*counter);
                    counter++;
                }
            }
            else
            {
                int counter = 0;
                foreach (var keyValue in _healthBarUis)
                {
                    RectTransform rectTransform = keyValue.Value.GameObject.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition += new Vector2(0,distanceBetweenOthers*counter);
                    counter++;
                }
            }
            
            
        }
    }
}