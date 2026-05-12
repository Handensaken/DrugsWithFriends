using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;
    
    public GameObject menuScreen;
    [SerializeField] private TMP_InputField lobbyInput;

    [SerializeField] private TextMeshProUGUI lobbyIDText, lobbyCodeText;
    
    [SerializeField] private GameObject startLobbyButton;

    [SerializeField] private GameObject startButton;
    private void Awake() => instance = this;

    private void Start()
    {
        DisplayLobbyCode();
        //EventSystem obj = FindAnyObjectByType<EventSystem>();
        //obj.gameObject.SetActive(false);
        //obj.gameObject.SetActive(true);
        //EventSystem.current = obj;
        //EventSystem.current.SetSelectedGameObject(startButton);
    }

    public void PlayButton(GameObject screenToClose)
    {
        screenToClose.SetActive(false);
    }

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
        //startLobbyButton.SetActive(true);
    }

    private void DisplayLobbyCode()
    {
        if (lobbyCodeText is null) return;
        lobbyCodeText.text = "Lobby Code: " + BootstrapManager.lobbyCode;
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.lobbyIDText.text = BootstrapManager.currentLobbyID.ToString();
        
        //StartLobby();
    }

    public static void CloseAllScreens()
    {
        instance.menuScreen.SetActive(false);
    }
    
    public void JoinLobby()
    {
        //CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(lobbyInput.text);
    }

    public void LeaveLobby()
    {
        
    }

    public static void StartLobby()
    {
        string[] scenesToClose = { "Main Menu" };
        BootstrapNetworkManager.ChangeNetworkScene("Game Lobby", scenesToClose);
    }

    public static void JoinStartedLobby()
    {
        string[] scenesToClose = { "Main Menu" };
        BootstrapNetworkManager.ChangeNetworkScene("Game Lobby", scenesToClose);
    }

    public static void StartGame()
    {
        string[] scenesToClose = { "Game Lobby" };
        BootstrapNetworkManager.ChangeNetworkScene("In Game Scene", scenesToClose);
    }
}
