using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField, Range(0, 1f)] private float rotationSpeed;
    private Vector2 rot;
    private Vector3 forwardVector;
    [SerializeField] private bool freeCamMovement;
    private bool looking, attacking;
    private Rigidbody rb;
    [SerializeField] private GameObject[] cameras;
    [SerializeField, Range(0, 10f)] private float range;
    [SerializeField, Range(0, 4f), Tooltip("Attacks per second")] private float attackSpeed;
    [SerializeField, Range(0, 4f), Tooltip("Time between attack chains")] private float lightChainAttackCooldown, heavyChainAttackCooldown;
    [SerializeField, Range(0, 2f), Tooltip("Time before when the next attack can be performed")] private float attackBufferTime;
    [SerializeField, Range(0, 5), Tooltip("Number of attacks in a chain")] private int maxChainLengthLight, maxChainLengthHeavy;

    [Serializable]
    struct ActionReferences // Jag vägrar göra string based lookup
    {
        public InputActionReference move, look, toggleCameraFocus, pause, unpause, cancel, lightAttack, heavyAttack;
    }
    
    [SerializeField] private ActionReferences actionReferences;
    [SerializeField] private PauseEvent pauseEvent;
    [SerializeField] private ControlSchemeEvent controlSchemeEvent;
    [SerializeField] private PlayerGameSettings playerSettings;
    private Animator animator;
    private int cameraIndex, enemyIndex;
    private List<Transform> enemiesInRange;
    
    private PlayerInput playerInput;
    private CinemachineCamera cinemachineCamera;
    private Vector3 moveVector;
    
    [SerializeField] private SelectionHandler selectionHandler;

    private void Awake()
    {
        freeCamMovement = true;
        cameraIndex = 0;
        enemiesInRange = new List<Transform>();
        playerInput = GetComponent<PlayerInput>();
        if (TryGetComponent(out Rigidbody rigidbody))
        {
            rb = rigidbody;
        }
        else
        {
            Debug.LogError("No Rigidbody found on PlayerNetwork object. Please add a Rigidbody component.");
        }

        if (TryGetComponent(out Animator anim))
        {
            animator = anim;
            animator.SetFloat("X-Input", 0);
            animator.SetFloat("Z-Input", 0);
        }
        else
        {
            Debug.LogError("Couldn't get animator");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (GetComponentInChildren<CinemachineCamera>() is CinemachineCamera vcam)
        {
            vcam.enabled = IsOwner;
            if (IsOwner)
            {
                cinemachineCamera = vcam;
            }
        }
        
        if (GetComponentInChildren<CinemachineInputAxisController>() is CinemachineInputAxisController axisController)
        {
            axisController.enabled = IsOwner;
        }
    }

    private void OnEnable()
    {
        actionReferences.move.action.performed += Move;
        actionReferences.move.action.canceled += Move;
        actionReferences.look.action.performed += Look;
        actionReferences.look.action.canceled += Look;
        actionReferences.toggleCameraFocus.action.performed += ToggleCameraFocus;
        actionReferences.lightAttack.action.performed += LightAttack;
        actionReferences.heavyAttack.action.performed += HeavyAttack;
        actionReferences.pause.action.performed += Pause;
        actionReferences.cancel.action.performed += Cancel; 
        actionReferences.unpause.action.performed += Unpause;
        playerInput.onControlsChanged += ControlsChanged;
    }

    private void OnDisable()
    {
        actionReferences.move.action.performed -= Move;
        actionReferences.move.action.canceled -= Move;
        actionReferences.look.action.performed -= Look;
        actionReferences.look.action.canceled -= Look;
        actionReferences.toggleCameraFocus.action.performed -= ToggleCameraFocus;
        actionReferences.lightAttack.action.performed -= LightAttack;
        actionReferences.heavyAttack.action.performed -= HeavyAttack;
        actionReferences.pause.action.performed -= Pause;
        actionReferences.unpause.action.performed -= Unpause;
        actionReferences.cancel.action.performed -= Cancel;
        playerInput.onControlsChanged -= ControlsChanged;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (attacking)
        {
            rb.linearVelocity = Vector2.zero;
        }
        if (looking)
        {
            forwardVector = cinemachineCamera.transform.forward;
            if(rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                SetVelocity();
            }
        }
        
        if (moveVector.sqrMagnitude > 0.01f)
        {
            Vector3 rotationTarget;
            if (!freeCamMovement)
            {
                rotationTarget = new Vector3(cinemachineCamera.transform.forward.x, 0f, cinemachineCamera.transform.forward.z).normalized;
            }
            else
            {
                rotationTarget = moveVector;
            }
    
            Quaternion targetRotation = Quaternion.LookRotation(rotationTarget);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed));
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

    private void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed)
        {
            SetVelocity();
        }
        else if (context.canceled)
        {
            rb.linearVelocity = new Vector3(0, 0, 0);
            animator.SetFloat("X-Input", 0);
            animator.SetFloat("Z-Input", 0);
        }
    }

    private void SetVelocity()
    {
        if(freeCamMovement)
        {
            FreeCamMovement();
        }
        else
        {
            TrackingObjectMovement();
        }
    }

    private void FreeCamMovement()
    {
        Vector2 direction = actionReferences.move.action.ReadValue<Vector2>();
        if (direction.sqrMagnitude < 0.01) return;

        Vector3 cameraForward = cinemachineCamera.transform.forward;
        Vector3 cameraRight = cinemachineCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        moveVector = (cameraForward * direction.y + cameraRight * direction.x);
        rb.linearVelocity = moveVector * moveSpeed;
        animator.SetFloat("Z-Input", 1);
    }

    private void TrackingObjectMovement()
    {
        Vector2 direction = actionReferences.move.action.ReadValue<Vector2>();
        if (direction.sqrMagnitude < 0.01) return;

        Vector3 cameraForward = cinemachineCamera.transform.forward;
        Vector3 cameraRight = cinemachineCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        moveVector = (cameraForward * direction.y + cameraRight * direction.x);
        rb.linearVelocity = moveVector * moveSpeed;
        animator.SetFloat("Z-Input", direction.y);
        animator.SetFloat("X-Input", direction.x);
    }

    private void Look(InputAction.CallbackContext context)
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
    
    private void ToggleCameraFocus(InputAction.CallbackContext context)
    {
        if (!IsOwner || cameras.Length < 2) return;

        if (enemiesInRange.Count == 0)
        {
            SwitchToCamera(0);
            freeCamMovement = true;
            return;
        }

        if (cameraIndex == 1)
        {
            enemyIndex = (enemyIndex + 1) % enemiesInRange.Count; // Goes back to zero if it's bigger than the list index 
        }
        else
        {
            SwitchToCamera(1);
            freeCamMovement = false;
        }
        
        cinemachineCamera.Target.TrackingTarget = enemiesInRange[enemyIndex];
    }
    
    private void SwitchToCamera(int index)
    {
        if (index == cameraIndex) return;
        cameras[cameraIndex].SetActive(false);
        cameraIndex = index;
        cameras[cameraIndex].SetActive(true);

        if (cameras[cameraIndex].TryGetComponent(out CinemachineCamera vcam))
        {
            cinemachineCamera = vcam;
        }
    }

    private void LightAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool("Light", true);
        }
    }
    
    private void HeavyAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool("Heavy", true);
        }
    }
    public void OnAttackStart()
    {
        actionReferences.move.action.Disable();
        rb.linearVelocity = Vector3.zero;
    }
    public void OnAttackEnd()
    {
        actionReferences.move.action.Enable();
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
