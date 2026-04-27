using System;
using Steamworks;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;
    
    public GameObject menuScreen;
    [SerializeField] private TMP_InputField lobbyInput;

    [SerializeField] private TextMeshProUGUI lobbyIDText;
    [SerializeField] private GameObject startLobbyButton;
    private void Awake() => instance = this;
    
    public void PlayButton(GameObject screenToClose)
    {
        screenToClose.SetActive(false);
    }

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
        startLobbyButton.SetActive(true);
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.lobbyIDText.text = BootstrapManager.currentLobbyID.ToString();
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
}
