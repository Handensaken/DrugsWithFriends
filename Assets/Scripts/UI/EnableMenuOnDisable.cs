using System;
using UnityEngine;

public class EnableMenuOnDisable : MonoBehaviour
{
    [SerializeField] private GameObject menuToEnable;
    private void OnDisable()
    {
        menuToEnable.SetActive(true);
    }
}
