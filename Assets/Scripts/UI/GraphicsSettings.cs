using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown, screenModeDropdown, fpsDropdown;
    [SerializeField] private Slider brightnessSlider;
    private Resolution[] resolutions;
    private List<string> resolutionsList;
    private int selectedResolution;
    private FullScreenMode fullscreenMode;
    //[SerializeField] private Volume postProcessingVolume;
    //private ColorAdjustments colorAdjustments;
    private readonly int[] fpsOptions = { 30, 60, 120, 144, 165, 240, -1 }; // -1 = Unlimited

    private void Awake()
    {
        //postProcessingVolume.profile.TryGet(out colorAdjustments);
        fullscreenMode = Screen.fullScreenMode;
        resolutions = Screen.resolutions;
        resolutionsList = new List<string>();

        foreach (Resolution res in resolutions)
        {
            string entry = $"{res.width} x {res.height}";
            if (!resolutionsList.Contains(entry))
                resolutionsList.Add(entry);
        }

        resolutionDropdown.options = resolutionsList.ConvertAll(res => new TMP_Dropdown.OptionData(res));

        string currentRes = $"{Screen.width} x {Screen.height}";
        selectedResolution = resolutionsList.IndexOf(currentRes);
        if (selectedResolution < 0) selectedResolution = 0;
        resolutionDropdown.SetValueWithoutNotify(selectedResolution);

        List<string> screenModesList = new List<string>();
        foreach (FullScreenMode mode in System.Enum.GetValues(typeof(FullScreenMode)))
            screenModesList.Add(mode.ToString());

        screenModeDropdown.options = screenModesList.ConvertAll(mode => new TMP_Dropdown.OptionData(mode));
        int currentModeIndex = screenModesList.IndexOf(fullscreenMode.ToString());
        screenModeDropdown.SetValueWithoutNotify(currentModeIndex >= 0 ? currentModeIndex : 0);

        brightnessSlider.value = Screen.brightness;

        // FPS dropdown setup
        List<string> fpsList = new List<string>();
        int currentFpsIndex = 0;
        for (int i = 0; i < fpsOptions.Length; i++)
        {
            fpsList.Add(fpsOptions[i] == -1 ? "Unlimited" : $"{fpsOptions[i]} FPS");
            if (fpsOptions[i] == Application.targetFrameRate) currentFpsIndex = i;
        }

        fpsDropdown.options = fpsList.ConvertAll(fps => new TMP_Dropdown.OptionData(fps));
        fpsDropdown.SetValueWithoutNotify(currentFpsIndex);
    }

    public void ChangeResolution(int index)
    {
        selectedResolution = index;
        Screen.SetResolution(resolutions[selectedResolution].width, resolutions[selectedResolution].height, fullscreenMode);
    }

    public void ChangeScreenMode(int index)
    {
        fullscreenMode = (FullScreenMode)index;
        Screen.SetResolution(resolutions[selectedResolution].width, resolutions[selectedResolution].height, fullscreenMode);
    }

    public void SetBrightness(float value)
    {
        /*
        if (colorAdjustments is null)
        {
            return;
        }
        colorAdjustments.postExposure.value = Mathf.Lerp(-2f, 2f, value);
        */
    } 

    public void SetMaxFPS(int index)
    {
        Application.targetFrameRate = fpsOptions[index];
    }
}