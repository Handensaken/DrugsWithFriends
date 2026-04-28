using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instance;
    private static string _currentGameScene = "";

    private void Awake()
    {
        instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerManager.OnRemoteConnectionState += OnClientConnected;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        ServerManager.OnRemoteConnectionState -= OnClientConnected;
    }

    private void OnClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            if (!string.IsNullOrEmpty(_currentGameScene))
            {
                SceneLoadData sld = new SceneLoadData(_currentGameScene);
                sld.ReplaceScenes = ReplaceOption.All;
                SceneManager.LoadConnectionScenes(conn, sld);
            }
        }
    }

    // Called from anywhere — routes correctly depending on who's calling
    public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
    {
        if (instance == null)
        {
            Debug.LogError("BootstrapNetworkManager instance not found.");
            return;
        }

        // If this is the server (or host), run directly
        if (instance.IsServerStarted)
        {
            instance.ServerChangeScene(sceneName, scenesToClose);
        }
        // If this is a client, ask the server to do it via ServerRpc
        else if (instance.IsClientStarted)
        {
            instance.RequestSceneChangeSrpc(sceneName, scenesToClose);
        }
        else
        {
            Debug.LogWarning("ChangeNetworkScene called but neither server nor client is active.");
        }
    }

    // Only a client would call this — server validates and executes
    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeSrpc(string sceneName, string[] scenesToClose)
    {
        // Add any authority check here if not all clients should be allowed to trigger this
        ServerChangeScene(sceneName, scenesToClose);
    }

    private void ServerChangeScene(string sceneName, string[] scenesToClose)
    {
        Debug.Log("Server changing scene to: " + sceneName);
        _currentGameScene = sceneName;

        NetworkConnection[] conns = ServerManager.Clients.Values.ToArray();

        SceneLoadData sld = new SceneLoadData(sceneName);
        sld.ReplaceScenes = ReplaceOption.None;

        SceneUnloadData sud = new SceneUnloadData(scenesToClose);

        SceneManager.LoadConnectionScenes(conns, sld);
        SceneManager.UnloadConnectionScenes(conns, sud);
    }
}