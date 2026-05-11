using UnityEngine;

public class InvertCameraControls : MonoBehaviour
{
    [SerializeField] private PlayerGameSettings playerGameSettings;
    
    public void ToggleInvertCamera(bool isOn)
    {
        playerGameSettings.invertMouseY = isOn;
    }
}
