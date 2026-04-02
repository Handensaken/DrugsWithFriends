using System;
using System.Collections.Generic;
using FishNet.Managing;
using UnityEngine;
using Steamworks;
using UnityEditor.Scripting;
using Random = UnityEngine.Random;

public class BootstrapManager : MonoBehaviour
{
    private static BootstrapManager instance;

    private void Awake() => instance = this;

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected CallResult<LobbyMatchList_t> LobbyMatchList;

    public static ulong currentLobbyID;
    private string lobbyCode;

    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
    }
    
    private string GenerateLobbyCode()
    {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    
            int codeLength = 6;
            List<char> codeChars = new List<char>();
    
            for (int i = 0; i < codeLength; i++)
            {
                char randomChar = chars[Random.Range(0, chars.Length)];
                codeChars.Add(randomChar);
            }
            return new string(codeChars.ToArray());
    }
    
    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("Starting lobby creation: " + callback.m_eResult.ToString());
        
        currentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
        lobbyCode = GenerateLobbyCode();
        Debug.Log("lobbycode is " + lobbyCode);
        SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), "lobbyCode", lobbyCode);
        fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        fishySteamworks.StartConnection(true);
        Debug.Log("Lobby creation was successful");
    }
    
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        currentLobbyID = callback.m_ulSteamIDLobby;

        MainMenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "name"), true);
        
        fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "HostAddress"));
        fishySteamworks.StartConnection(false);
    }

    public static void JoinByID(string ID)
    {
        Debug.Log("Attempting to jioin lobbyu with id" + ID);

        SteamMatchmaking.AddRequestLobbyListStringFilter("lobbyCode", ID, ELobbyComparison.k_ELobbyComparisonEqual);
        //SteamAPICall_t lobbyList = SteamMatchmaking.RequestLobbyList();
        SteamMatchmaking.RequestLobbyList();
        //LobbyMatchList.Set(lobbyList);
    }
    
    void OnLobbyMatchList(LobbyMatchList_t pLobbyMatchList, bool bIOFailure )
    {
        if(bIOFailure)
        {
            Debug.Log("Failed to retrieve lobby list.");
            return;
        }
        if(pLobbyMatchList.m_nLobbiesMatching == 0)
        {
            Debug.Log("No lobbies found with code: " + lobbyCode);
            return;
        }
        CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(0);
        Debug.Log("Lobby found with code: " + lobbyCode + ", joining lobby with ID: " + lobbyID);
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyID));
        currentLobbyID = 0;

        instance.fishySteamworks.StopConnection(false);
        if (instance.networkManager.IsServer)
        {
            instance.fishySteamworks.StopConnection(true);
        }
    }
}
