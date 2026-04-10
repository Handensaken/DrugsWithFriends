using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerGameSettings")]
public class PlayerGameSettings : ScriptableObject
{
    public const float minSensitivity = 0f;
    public const float maxSensitivity = 10f;
    [Range(minSensitivity, maxSensitivity)] public float mouseSensitivity;
    public float volume = 1.0f;
    public bool invertMouseY = false;
    
    [HideInInspector] public UnityEvent OnSensitivityChanged = new UnityEvent();

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey("MouseSensitivity"))
        {
            Debug.Log("Set sense to " + PlayerPrefs.GetFloat("MouseSensitivity"));
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
            OnSensitivityChanged.Invoke();
        }
        
        if (PlayerPrefs.HasKey("Volume"))
        {
            volume = PlayerPrefs.GetFloat("Volume");
        }
        
        if (PlayerPrefs.HasKey("InvertMouseY"))
        {
            invertMouseY = PlayerPrefs.GetInt("InvertMouseY") != 0; // Converts to bool
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.SetInt("InvertMouseY", invertMouseY ? 1 : 0);
        PlayerPrefs.Save(); 
    }
}