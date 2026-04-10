using System;
using Unity.Cinemachine;
using UnityEngine;

public class SetCameraSensitivity : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private PlayerGameSettings playerGameSettings;

    private void OnEnable()
    {
        playerGameSettings.OnSensitivityChanged.AddListener(SetSensitivity);
    }

    private void OnDisable()
    {
        playerGameSettings.OnSensitivityChanged.RemoveListener(SetSensitivity);
    }

    private void Start()
    {
        SetSensitivity();
    }

    private void SetSensitivity()
    {
        Debug.Log("SET SENSE");
        var inputController = virtualCamera.GetComponent<CinemachineInputAxisController>();
        if (inputController != null)
        {
            foreach (var controller in inputController.Controllers)
            {
                controller.Input.Gain = playerGameSettings.mouseSensitivity;
            }
        }
    }
}
