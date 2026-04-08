using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabMenu : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;
    [SerializeField] private ControlSchemeEvent controlSchemeEvent;
    [SerializeField] private SelectionHandler selectionHandler;

    private void OnEnable()
    {
        if (!controlSchemeEvent.currentControlScheme.Contains("keyboard") && controlSchemeEvent.currentControlScheme is not null)
        {
            selectionHandler.selectedObjects.Add(firstSelected);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    private void OnDisable()
    {
        try
        {
            foreach (var obj in selectionHandler.selectedObjects)
            {
                if (obj == firstSelected)
                {
                    selectionHandler.selectedObjects.Remove(obj);
                    break;
                }
            }
        }
        catch
        {
        }
    }
}