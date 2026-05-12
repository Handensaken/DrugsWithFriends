using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown, screenModeDropdown;
    private Resolution[] resolutions;
    private List<string> resolutionsList;
    private int selectedResolution;

    private FullScreenMode fullscreenMode = FullScreenMode.FullScreenWindow;

    private void Awake()
    {
        resolutions = Screen.resolutions;
        resolutionsList = new List<string>();

        foreach (Resolution res in resolutions)
        {
            string entry = $"{res.width} x {res.height}";
            if (!resolutionsList.Contains(entry))
                resolutionsList.Add(entry);
        }

        resolutionDropdown.options = resolutionsList.ConvertAll(res => new TMP_Dropdown.OptionData(res));

        List<string> screenModesList = new List<string>();

        foreach (FullScreenMode mode in System.Enum.GetValues(typeof(FullScreenMode)))
        {
            screenModesList.Add(mode.ToString());
        }

        screenModeDropdown.options = screenModesList.ConvertAll(mode => new TMP_Dropdown.OptionData(mode));
        screenModeDropdown.value = screenModesList.IndexOf(fullscreenMode.ToString());
    }

    public void ChangeResolution()
    {
        selectedResolution = resolutionDropdown.value;
        Screen.SetResolution(resolutions[selectedResolution].width, resolutions[selectedResolution].height, fullscreenMode);
    }

    public void ChangeFullscreenMode(int index)
    {
        fullscreenMode = (FullScreenMode)index;
        Screen.SetResolution(resolutions[selectedResolution].width, resolutions[selectedResolution].height, fullscreenMode);
    }
}