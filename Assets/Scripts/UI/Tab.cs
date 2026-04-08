using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tab : MonoBehaviour
{
    public GameObject[] menus;

    public void ActivateMenu(GameObject menuToOpen)
    {
        foreach (GameObject menu in menus)
        {
            if (menu != menuToOpen)
            {
                menu.SetActive(false);
            }
            else
            {
                menu.SetActive(!menu.activeSelf);
            }
        }
    }
}