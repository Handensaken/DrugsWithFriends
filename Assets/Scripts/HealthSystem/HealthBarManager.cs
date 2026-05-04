using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scenes.Dev_Scenes.Patrik.HealthSystem
{
    public class HealthBarManager : NetworkBehaviour
    {
          [SerializeField] private GameObject healthBarForOtherPlayers;

          [SerializeField] private HealthBarUI playerHealthBarUI;
          [SerializeField] private List<HealthBarUI> healthBarUis = new List<HealthBarUI>();

          public override void OnStartServer()
          {
              ServerManager.OnRemoteConnectionState += HandleConnectionChangeFromOtherClient;
          }

          public override void OnStartClient()
          {
              base.OnStartClient();
              playerHealthBarUI.ID = ClientManager.Connection.ClientId;
              SetUpAllExtraBars();
          }

          public override void OnStopClient()
          {
              base.OnStopClient();
              
              for (int i = 0; i < healthBarUis.Count; i++)
              {
                  Debug.Log("Remove: "+i);
                  Destroy(healthBarUis[^(i+1)].gameObject);
              }
              healthBarUis.Clear();
          }

          private void SetUpAllExtraBars()
          {
              List<int> ids = new List<int>();
              foreach (NetworkConnection networkConnection in ClientManager.Clients.Values)
              {
                  if (networkConnection.ClientId == ClientManager.Connection.ClientId)
                  {
                      continue;
                  }
                  ids.Add(networkConnection.ClientId);
              }
              CreateBars(ids.ToArray());
              
              MoveHealthBars();
          }
          
          private void HandleConnectionChangeFromOtherClient(NetworkConnection networkConnection, RemoteConnectionStateArgs remoteConnectionStateArgs)
          {
              Debug.Log("Check");
              
              if (remoteConnectionStateArgs.ConnectionState == RemoteConnectionState.Started)
              {
                  Debug.Log("Client joined: "+networkConnection.ClientId);
                  HandleAddingBar(networkConnection.ClientId);
              }
              else if (remoteConnectionStateArgs.ConnectionState == RemoteConnectionState.Stopped)
              {
                  Debug.Log("Client left: "+networkConnection.ClientId);
                  HandleRemovalOfBar(networkConnection.ClientId);
              }
          }

          private void CreateBars(int[] barAndClientID)
          {
              int amountOfBars = barAndClientID.Length;
              for (int i = 0; i < amountOfBars; i++)
              {
                  Debug.Log("Add: "+i);
                  HealthBarUI test = Instantiate(healthBarForOtherPlayers,gameObject.GetComponent<RectTransform>()).GetComponent<HealthBarUI>();
                  
                  test.ID = barAndClientID[i];
                  healthBarUis.Add(test);
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
              for (int i = 0; i < healthBarUis.Count; i++)
              {
                  HealthBarUI currentHealthBar = healthBarUis[^(i+1)];
                  if (currentHealthBar.ID == clientID)
                  {
                      healthBarUis.Remove(currentHealthBar);
                      Destroy(currentHealthBar.gameObject);
                      return;
                  }
              }
          }
          
          [ObserversRpc]
          private void HandleAddingBar(int clientID)
          {
              CreateBars(new []{clientID});
              MoveHealthBars();
          }
          
          [ObserversRpc]
          private void HandleRemovalOfBar(int clientID)
          {
              RemoveHealthBars(clientID);
              MoveHealthBars();
          }
    }
}