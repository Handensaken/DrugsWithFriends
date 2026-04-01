using System;
using Steamworks;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;
    
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private TMP_InputField lobbyInput;

    [SerializeField] private TextMeshProUGUI lobbyIDText;
    private void Awake() => instance = this;

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.lobbyIDText.text = BootstrapManager.currentLobbyID.ToString();
    }

    private void CloseAllScreens()
    {
        menuScreen.SetActive(false);
    }
    
    public void JoinLobby()
    {
        //CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(lobbyInput.text);
    }

    public void LeaveLobby()
    {
        
    }

    public void StartGame()
    {
        string[] scenesToClose = { "InsertMainMenuNameHereLater" };
        BootstrapNetworkManager.ChangeNetworkScene("InsertGameSceneNameHereLater", scenesToClose);
    }
}
