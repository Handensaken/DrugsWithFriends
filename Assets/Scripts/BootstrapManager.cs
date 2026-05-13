using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Managing.Transporting;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using Steamworks;
//using UnityEditor.Scripting;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BootstrapManager : MonoBehaviour
{
    [HideInInspector] public static BootstrapManager instance;
    [SerializeField] private TransportManager transportManager;
    [SerializeField] private Tugboat tugboat;
    public bool useSteam;
    
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected CallResult<LobbyMatchList_t> LobbyMatchList;

    public static ulong currentLobbyID;
    public static string lobbyCode;
    public static bool isHost;

    private void OnValidate()
    {
        CheckTransport();
    }

    private void Awake()
    {
        instance = this;
        CheckTransport();
    }
    
    private void Start()
    {
        isHost = false;
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
        if (SceneManager.GetActiveScene().name == "Bootstrap")
        {
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Additive);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        //Debug.Log(networkManager.TimeManager.RoundTripTime);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu" || scene.name == "HannesScene") return;

        var mainMenu = SceneManager.GetSceneByName("Main Menu");
        if (mainMenu.isLoaded)
        {
            SceneManager.UnloadSceneAsync(mainMenu);
        }
        
        if(scene.name == "Game Lobby") return;
        
        var gameLobby = SceneManager.GetSceneByName("Game Lobby");
        if (gameLobby.isLoaded)
        {
            SceneManager.UnloadSceneAsync(gameLobby);
        }
    }

    private void CheckTransport()
    {
        if(useSteam)
        {
            transportManager.Transport = fishySteamworks;
            fishySteamworks.enabled = true;
        }
        else
        {
            transportManager.Transport = tugboat;
            fishySteamworks.enabled = false;
        }
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
        if (instance.useSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
            MainMenuManager.CloseAllScreens();
        }
        else
        {
            Debug.Log("creating lobby with tugboat");
            instance.tugboat.StartConnection(true);
            instance.tugboat.StartConnection(false);
            MainMenuManager.CloseAllScreens();
            instance.tugboat.OnClientConnectionState += StartLobbyTugboat;
        }
        lobbyCode = instance.GenerateLobbyCode();
        Debug.Log("lobbycode is " + lobbyCode);
        isHost = true;
    }

    private static void StartLobbyTugboat(ClientConnectionStateArgs t)
    {
        if(t.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("starting lobby with tugboat");
            
            if(SceneManager.GetSceneByName("Main Menu").isLoaded)
            {
                MainMenuManager.StartLobby();
            }
            instance.networkManager.ClientManager.OnClientConnectionState -= StartLobbyTugboat;
        }
    }
    
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("Starting lobby creation: " + callback.m_eResult.ToString());
        
        currentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(currentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
        //lobbyCode = GenerateLobbyCode();
        //Debug.Log("lobbycode is " + lobbyCode);
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
        Debug.Log("lobby entered");
        currentLobbyID = callback.m_ulSteamIDLobby;
        MainMenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "name"), true);
        fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "HostAddress"));
        fishySteamworks.StartConnection(false);
        
        instance.fishySteamworks.OnClientConnectionState += OnSteamClientConnected;
    }
    
    private static void OnSteamClientConnected(ClientConnectionStateArgs t)
    {
        if (t.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("Steam connection established");
            instance.fishySteamworks.OnClientConnectionState -= OnSteamClientConnected;
            instance.fishySteamworks.OnClientConnectionState += ConnectionLostSteam;
            MainMenuManager.JoinStartedLobby();
        }
    }
    
    private static void ConnectionLostSteam(ClientConnectionStateArgs t)
    {
        if (t.ConnectionState == LocalConnectionState.Stopped)
        {
            LeaveLobby();
        }
    }

    public static void JoinByID(string ID)
    {
        if (!instance.useSteam)
        {
            instance.tugboat.StartConnection(false);
            MainMenuManager.CloseAllScreens();
            instance.tugboat.OnClientConnectionState += JoinLobbyTugboat;
        }
        else
        {
            Debug.Log("Attempting to jioin lobbyu with id" + ID);
            SteamMatchmaking.AddRequestLobbyListStringFilter("lobbyCode", ID, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamAPICall_t lobbyList = SteamMatchmaking.RequestLobbyList();
            instance.LobbyMatchList.Set(lobbyList);
            MainMenuManager.CloseAllScreens();
        }
    }
    
    private static void JoinLobbyTugboat(ClientConnectionStateArgs t)
    {
        if(t.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("starting lobby with tugboat");
            MainMenuManager.JoinStartedLobby();
            instance.networkManager.ClientManager.OnClientConnectionState -= JoinLobbyTugboat;
        }

        instance.networkManager.ClientManager.OnClientConnectionState += ConnectionLostTugboat;
    }
    
    private static void ConnectionLostTugboat(ClientConnectionStateArgs t)
    {
        if(t.ConnectionState == LocalConnectionState.Stopped)
        {
            //LeaveLobby();
        }
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
        if (instance.useSteam)
        {
            //SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
            SteamMatchmaking.LeaveLobby(new CSteamID(currentLobbyID));
            
            instance.fishySteamworks.StopConnection(false);
            if (instance.networkManager.IsServer)
            {
                instance.fishySteamworks.StopConnection(true);
            }
        }
        else
        {
            Debug.Log("Leaving lobby with tugboat");
            if(isHost)
            {
                string[] scenesToClose = { "In Game Scene" };
                BootstrapNetworkManager.ChangeNetworkScene("Main Menu", scenesToClose);
                instance.tugboat.StopConnection(true);
                instance.tugboat.StopConnection(false);
            }
            else
            {
                string[] scenesToClose = { "In Game Scene" };
                BootstrapNetworkManager.ChangeNetworkScene("Main Menu", scenesToClose);
                instance.tugboat.StopConnection(false);
            }
        }
        currentLobbyID = 0;
    }
}
