using UnityEngine;

public class StartLobbyButton : MonoBehaviour
{
    private void OnEnable()
    {
        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        gameObject.SetActive(BootstrapManager.isHost);
    }
}
