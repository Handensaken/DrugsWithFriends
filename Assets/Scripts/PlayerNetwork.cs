using System;
using System.Collections;
using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(PlayerInput))]
public class PlayerNetwork : NetworkBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;

    private Vector2 rot;
    private bool moving, looking;

    [Serializable]
    struct ActionReferences // Jag vägrar göra string based lookup
    {
        public InputActionReference move, look, pause, unpause, cancel;
    }
    
    [SerializeField] private ActionReferences actionReferences;
    [SerializeField] private PauseEvent pauseEvent;
    [SerializeField] private ControlSchemeEvent controlSchemeEvent;
    [SerializeField] private PlayerGameSettings playerSettings;
    
    private PlayerInput playerInput;
    
    [SerializeField] private SelectionHandler selectionHandler;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }
    

    public override void OnStartClient()
    {
        Debug.Log("started client");
        base.OnStartClient();

        StartCoroutine(InitializeCamera());
    }

    private IEnumerator InitializeCamera()
    {
        yield return null;
        
        if (GetComponentInChildren<CinemachineCamera>() is CinemachineCamera vcam)
            vcam.enabled = IsOwner;

        if (GetComponentInChildren<CinemachineInputAxisController>() is CinemachineInputAxisController axisController)
        {
            axisController.enabled = IsOwner;
            if (IsOwner)
                axisController.PlayerIndex = playerInput.playerIndex;
        }
    }
    

    private void OnEnable()
    {
        actionReferences.move.action.performed +=  AllowMove;
        actionReferences.move.action.canceled += AllowMove;
        actionReferences.look.action.performed += AllowLook;
        actionReferences.look.action.canceled += AllowLook;
        actionReferences.pause.action.performed += Pause;
        actionReferences.cancel.action.performed += Cancel; 
        actionReferences.unpause.action.performed += Unpause;
        playerInput.onControlsChanged += ControlsChanged;
    }

    private void OnDisable()
    {
        actionReferences.move.action.performed -= AllowMove;
        actionReferences.move.action.canceled -= AllowMove;
        actionReferences.look.action.performed -= AllowLook;
        actionReferences.look.action.canceled -= AllowLook;
        actionReferences.pause.action.performed -= Pause;
        actionReferences.unpause.action.performed -= Unpause;
        actionReferences.cancel.action.performed -= Cancel;
    }

    public void Update()
    {
        if (!IsOwner) return;
        if (moving)
        {
            Move();
        }

        if (looking)
        {
            //Look();
        }
    }
    
    public void ResumeButton()
    {
        Unpause(new InputAction.CallbackContext());
    }

    private void Pause(InputAction.CallbackContext context)
    {
        pauseEvent.OnPause?.Invoke(playerInput);
    }

    private void Unpause(InputAction.CallbackContext context)
    {
        if (actionReferences.cancel.action.triggered) return; // Prevent unpausing when cancel is pressed in case cancel is bound to the same key as pause
        pauseEvent.OnUnpause?.Invoke(playerInput);
    }

    private void Cancel(InputAction.CallbackContext context)
    {
        pauseEvent.OnCancel?.Invoke(playerInput);
    }

    private void AllowMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moving = true;
        }
        else if (context.canceled)
        {
            moving = false;
        }
    }
    
    private void AllowLook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            looking = true;
        }
        else if (context.canceled)
        {
            looking = false;
        }
    }

    private void Move()
    {
        Vector2 direction = actionReferences.move.action.ReadValue<Vector2>();
        if (direction.sqrMagnitude < 0.01) return;
        var scaledMoveSpeed = moveSpeed * Time.deltaTime;
        var moveVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * new Vector3(direction.x, 0, direction.y);
        transform.position += moveVector * scaledMoveSpeed;
    }

    private void Look()
    {
        Debug.Log("looking");
        Vector2 rotate = actionReferences.look.action.ReadValue<Vector2>();
        if (rotate.sqrMagnitude < 0.01)
            return;
        int invert = playerSettings.invertMouseY ? -1 : 1;
        var scaledRotateSpeed = invert * rotateSpeed * Time.deltaTime * playerSettings.mouseSensitivity;
        rot.y += rotate.x * scaledRotateSpeed;
        rot.x = Mathf.Clamp(rot.x - rotate.y * scaledRotateSpeed, -45, 45);
        transform.localEulerAngles = rot;
    }
    
    private void ControlsChanged(PlayerInput input)
    {
        controlSchemeEvent.currentControlScheme = input.currentControlScheme.ToLower();
        if(!controlSchemeEvent.currentControlScheme.Contains("keyboard") && controlSchemeEvent.currentControlScheme is not null)
        {
            // Set selected eventsystem object to the last position of the selection handler
            var selectedObjects = selectionHandler.selectedObjects;
            if (selectedObjects.Count > 0)
            {
                if (selectionHandler.selectedObjects[selectedObjects.Count - 1] is null) return;
                EventSystem.current.SetSelectedGameObject(selectedObjects[selectedObjects.Count - 1]);
            }
        }
        else if (controlSchemeEvent.currentControlScheme.Contains("keyboard") && controlSchemeEvent.currentControlScheme is not null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
