using System;
using Steamworks;
using TMPro;
using UnityEngine;

public class LobbyHandler : MonoBehaviour
{
    private LobbyHandler instance;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    private void Awake() => instance = this;

    private void Start()
    {
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(BootstrapManager.currentLobbyID), "name");
    }
}
