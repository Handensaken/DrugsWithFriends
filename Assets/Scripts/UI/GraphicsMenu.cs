using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraphicsMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    private Resolution[] resolutions;
    private List<String> availableResolutions;
    void Start()
    {
        resolutions = Screen.resolutions;
        availableResolutions = new List<String>();
        
        resolutionDropdown.ClearOptions();
        

        foreach (var res in resolutions)
        {
            availableResolutions.Add(res.ToString());
        }
        resolutionDropdown.AddOptions(availableResolutions);
    }

    public void SetResolution(int resolutionsIndex)
    {
        Resolution resolution = resolutions[resolutionsIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}