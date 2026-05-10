using UnityEngine;

public class GameHandler : MonoBehaviour
{
    private static GameHandler instance;
    
    private void Awake() => instance = this;
    
    public void ReturnToMainMenu()
    {
        BootstrapManager.LeaveLobby();
        
        Debug.Log("leaving lobby");
    }
}
