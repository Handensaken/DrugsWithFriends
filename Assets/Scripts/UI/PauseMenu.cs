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
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject firstSelected;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject optionsMenu;
    private Menu[] menus;
    [SerializeField] private SelectionHandler selectionHandler;
    [SerializeField] private ControlSchemeEvent controlSchemeEvent;

    private void OnEnable()
    {
        selectionHandler.selectedObjects.Add(firstSelected);
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

    private void Awake()
    {
        pauseEvent.OnPause.AddListener(OnPause);
        pauseEvent.OnUnpause.AddListener(OnUnpause);
        pauseEvent.OnCancel.AddListener(TryCancel);
    }

    private void Start()
    {
        menus = optionsMenu.GetComponentsInChildren<Menu>(true).Where(menu => menu.gameObject != optionsMenu.gameObject).ToArray();
        pauseMenu.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPause(PlayerInput playerInput)
    {
        playerInput.SwitchCurrentActionMap("UI");
        pauseMenu.SetActive(true);
        
        if (!playerInput.currentControlScheme.ToLower().Contains("keyboard"))
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    public void OnUnpause(PlayerInput playerInput)
    {
        playerInput.SwitchCurrentActionMap("Player");
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
    }

    private void TryCancel(PlayerInput playerInput)
    {
        foreach (var menu in menus)
        {
            if (menu.gameObject.activeSelf)
            {
                menu.gameObject.SetActive(false);
                SetSelectedObject();
                return;
            }
        }
        if (optionsMenu.activeSelf)
        {
            optionsMenu.SetActive(false);
            SetSelectedObject();
        }
        else if (pauseMenu.activeSelf)
        {
            OnUnpause(playerInput);
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
