using System;
using FishNet.Managing;
using UnityEngine;
using Steamworks;

public class BootstrapManager : MonoBehaviour
{
    private static BootstrapManager instance;

    private void Awake() => instance = this;

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }
    
    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("Starting lobby creation: " + callback.m_eResult.ToString());
    }
    
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        
    }
    
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        
    }
}
