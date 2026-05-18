using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown, screenModeDropdown;
    private Resolution[] resolutions;
    private List<string> resolutionsList;
    private int selectedResolution;

    private FullScreenMode fullscreenMode;

    private void Awake()
    {
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
}