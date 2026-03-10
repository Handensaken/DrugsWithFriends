using System.Collections.Generic;
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using Netcode.Transports;
using Unity.Netcode;
using UnityEngine;
using Steamworks;
using TMPro;
using Random = UnityEngine.Random;

public class NetworkManagerUISteamworks : MonoBehaviour
{
    private SteamNetworkingSocketsTransport steamTransport;
    private SteamManager steamManager;

    private void Start()
    {
        steamManager = NetworkManager.Singleton.GetComponent<SteamManager>();
        steamTransport = NetworkManager.Singleton.GetComponent<SteamNetworkingSocketsTransport>();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        //var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //string hostAddress = transport.GetLocalEndpoint().Address.ToString();
        //SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
        SteamLobbyModeType lobbyType = SteamLobbyModeType.General;
        ELobbyType type = ELobbyType.k_ELobbyTypePublic;
        int slots = 4;
        LobbyData.Create(type, lobbyType, slots, HandleLobbyCreate);
    }

    void HandleLobbyCreate(EResult result, LobbyData lobby, bool ioError)
    {
        string lobbyCode = GenerateLobbyCode();
        Debug.Log(lobbyCode);
        lobby.SetLobbyMetadata("code", lobbyCode);
        steamTransport.ConnectToSteamID = lobby.Owner.user.id.m_SteamID;
        lobby.SetLobbyMetadata("id", steamTransport.ConnectToSteamID.ToString());
        lobby.SetGameServer();
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
    
    public void JoinWithLobbyCode(TextMeshProUGUI textMeshProUGUI)
    {
        string lobbyCode = textMeshProUGUI.text.ToUpper().Trim().Replace("\u200B", "").ToUpperInvariant();
        //string lobbyCode = textMeshProUGUI.text.ToUpper();
        SearchArguments args = new();
        args.stringFilters.Add(new() { key = "code", value = lobbyCode, comparison = ELobbyComparison.k_ELobbyComparisonEqual });
        LobbyData.Request(args, 1, HandleResults);
    }

    private void HandleResults(LobbyData[] lobbies, bool IOError)
    {
        if (lobbies.Length == 1)
        {
            steamTransport.ConnectToSteamID = ulong.Parse(lobbies[0].GetMetadata()["id"]);
            Debug.Log(steamTransport.ConnectToSteamID);
            LobbyData.Join(lobbies[0].AccountId, HandleJoin);
            Matchmaking.Client.JoinLobby(lobbies[0], HandleJoin);
        }
    }

    private void HandleJoin(LobbyEnter result, bool IOError)
    {
        Debug.Log("joined lobby");
        LobbyGameServer server = result.Lobby.GameServer;

        CSteamID serverID = server.id;
        string ipAdress = server.IpAddress;
        ushort port = server.port;
        
        result.Lobby.SetGameServer(ipAdress, port, serverID);
        NetworkManager.Singleton.StartClient();
    }
}