using FishNet.Managing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    private static GameHandler instance;
    
    private void Awake() => instance = this;
    
    public void ReturnToMainMenu()
    {
        BootstrapManager.LeaveLobby();
    
        // Destroy all DontDestroyOnLoad objects that need to be reset
        var networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager != null)
            Destroy(networkManager.gameObject);
    }

    private void OpenMainMenu()
    {
        SceneManager.LoadScene("Bootstrap");
    }

    private void OnMainMenuLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main Menu") return;

        SceneManager.sceneLoaded -= OnMainMenuLoaded;
        SceneManager.SetActiveScene(scene);

        // Ensure there's a valid EventSystem in the new scene
        if (EventSystem.current == null)
        {
            Debug.LogWarning("No EventSystem found in Main Menu scene — add one.");
        }
    }
}
