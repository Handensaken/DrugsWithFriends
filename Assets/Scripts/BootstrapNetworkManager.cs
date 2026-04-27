using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instance;
    
    private void Awake()
    {
        instance = this;
    }

    public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
    {
        Debug.Log("Changing scene");
        instance.CloseScenes(scenesToClose);
        SceneLoadData sld = new SceneLoadData(sceneName);
        NetworkConnection[] conns = instance.ServerManager.Clients.Values.ToArray();
        instance.SceneManager.LoadConnectionScenes(conns, sld);
    }
    
    private void CloseScenes(string[] scenesToClose)
    {
        CloseScenesObserver(scenesToClose);
    }
    
    private void CloseScenesObserver(string[] scenesToClose)
    {
        Debug.Log("closing scenes");
        foreach (var scene in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }
    }
}
