using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private PlayerGameSettings playerSettings;

    private void Start()
    {
        slider = GetComponent<Slider>();
        if (slider is not null)
        {
            slider.minValue = PlayerGameSettings.minSensitivity; // Since it's a const that is set in the script it's fine to do this
            slider.maxValue = PlayerGameSettings.maxSensitivity;
            slider.value = playerSettings.mouseSensitivity;
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        playerSettings.mouseSensitivity = sensitivity;
        playerSettings.OnSensitivityChanged.Invoke();
    }
}
