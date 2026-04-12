using System;
using UnityEngine;

public class EnableMenuOnDisable : MonoBehaviour
{
    [SerializeField] private GameObject menuToEnable;
    private void OnDisable()
    {
        Debug.Log("enabling pausemenu");
        menuToEnable.SetActive(true);
    }
}
