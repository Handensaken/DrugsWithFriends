using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private PauseEvent pauseEvent;
    [SerializeField] private GameObject pauseMenu, firstSelected, optionsMenu, menuButtons;
    private Menu[] menus;
    [SerializeField] private SelectionHandler selectionHandler;
    [SerializeField] private ControlSchemeEvent controlSchemeEvent;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputActionReference pause, unpause, cancel, spectatorPause, look;
    [SerializeField, ReadOnly] private string currentActionMap;
    
    [HideInInspector] public string previousActionMap;

    private void OnEnable()
    {
        selectionHandler.selectedObjects.Add(firstSelected);
        unpause.action.performed += OnUnpause;
        cancel.action.performed += TryCancel;
        pause.action.performed -= OnPause;
        spectatorPause.action.performed -= OnPause;
        
        try
        {
            previousActionMap = playerInput.currentActionMap.name;
        }
        catch
        {
            Debug.Log("No current actionmap");
            throw;
        }
        
        playerInput.SwitchCurrentActionMap("UI");
        currentActionMap = playerInput.currentActionMap.name;
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
        unpause.action.performed -= OnUnpause;
        cancel.action.performed -= TryCancel;
        pause.action.performed += OnPause;
        spectatorPause.action.performed += OnPause;
        
        if (playerInput != null && playerInput.isActiveAndEnabled)
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);
        }
        currentActionMap = playerInput.currentActionMap.name;
    }

    private void OnDestroy()
    {
        unpause.action.performed -= OnUnpause;
        cancel.action.performed -= TryCancel;
        pause.action.performed -= OnPause;
    }

    private void Start()
    {
        menus = pauseMenu.GetComponentsInChildren<Menu>(true).ToArray();
        pauseMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(pauseMenu is null) return;
        pauseMenu.SetActive(true);
        
        if (!playerInput.currentControlScheme.ToLower().Contains("keyboard"))
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }
    
    public void Resume()
    {
        OnUnpause(new InputAction.CallbackContext());
    }

    public void OnUnpause(InputAction.CallbackContext context)
    {
        foreach (var menu in menus)
        {
            menu.gameObject.SetActive(false);
        }
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Options()
    {
        optionsMenu.SetActive(!optionsMenu.activeSelf);
        if (optionsMenu.activeSelf)
        {
            menuButtons.SetActive(false);
        }
    }

    private void TryCancel(InputAction.CallbackContext context)
    {
        for (int i = menus.Length - 1; i >= 0; i--)
        {
            if (menus[i].gameObject.activeSelf)
            {
                menus[i].gameObject.SetActive(false);
                SetSelectedObject();
                return;
            }
        }
        if (menuButtons.activeSelf)
        {
            OnUnpause(new InputAction.CallbackContext());
        }
    }

    private void SetSelectedObject()
    {
        if(!controlSchemeEvent.currentControlScheme.Contains("keyboard") && controlSchemeEvent.currentControlScheme is not null)
        {
            // Set selected eventsystem object to the last position of the selection handler
            if (selectionHandler.selectedObjects.Count > 0)
            {
                if (selectionHandler.selectedObjects[selectionHandler.selectedObjects.Count - 1] is null) return;
                EventSystem.current.SetSelectedGameObject(selectionHandler.selectedObjects[selectionHandler.selectedObjects.Count - 1]);
            }
        }
    }
}
