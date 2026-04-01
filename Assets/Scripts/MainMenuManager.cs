using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;
    
    private void Awake() => instance = this;

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }
}
