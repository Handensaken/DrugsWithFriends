using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraSettings : MonoBehaviour
{
    [SerializeField] List<CinemachineCamera> cameras;
    [SerializeField] Slider fovSlider;
    private void Start()
    {
        fovSlider.value = cameras[0].Lens.FieldOfView;
    }

    public void SetCameraFOV(float fov)
    {
        foreach (var cinemachineCamera in cameras)
        {
            cinemachineCamera.Lens.FieldOfView = fov;
        }
    }
}
