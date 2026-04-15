using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Animating;
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
    [SerializeField, Range(0, 1f), Tooltip("Time before when the next attack can be performed")] private float attackBufferTime;
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
    private NetworkAnimator networkAnimator;
    private int cameraIndex, enemyIndex, currentChain;
    private List<Transform> enemiesInRange;
    private Queue<string> attackQueue;
    private PlayerInput playerInput;
    private CinemachineCamera cinemachineCamera;
    private Vector3 moveVector;
    private float attackQueueTimestamp = -1f;
    
    [SerializeField] private SelectionHandler selectionHandler;

    private void Awake()
    {
        freeCamMovement = true;
        cameraIndex = 0;
        currentChain = 0;
        enemiesInRange = new List<Transform>();
        attackQueue = new Queue<string>();
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
        
        if (TryGetComponent(out NetworkAnimator nAnim))
        {
            networkAnimator = nAnim;
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
        Debug.Log(currentChain);
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
            HandleRotation();
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

    private void HandleRotation()
    {
        Vector3 rotationTarget;
        if (!freeCamMovement)
        {
            rotationTarget = new Vector3(cinemachineCamera.transform.forward.x, 0f, cinemachineCamera.transform.forward.z).normalized;
        }
        else if(moveVector.sqrMagnitude > 0.01f)
        {
            rotationTarget = moveVector;
        }
        else
        {
            return;
        }
    
        Quaternion targetRotation = Quaternion.LookRotation(rotationTarget);
            
        float angleDifference = Quaternion.Angle(rb.rotation, targetRotation);

        if (angleDifference > 180)
        {
            animator.SetBool("isTurning", true);
        }
        else
        {
            animator.SetBool("isTurning", false);
        }
            
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed));
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
        if (!IsOwner) return;
        if(cameraIndex == 0)
        {
            SwitchToCamera(1);
            freeCamMovement = false;
        }
        else
        {
            SwitchToCamera(0);
            freeCamMovement = true;
        }
        /*
        if (enemiesInRange.Count == 0)
        {
            SwitchToCamera(0); // Should recenter camera when there is no other camera to switch to 
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
        */
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
        if (!IsOwner) return;
        if (context.performed)
        {
            QueueLightAttack("Light");
        }
    }
    
    private void HeavyAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed)
        {
            QueueHeavyAttack("Heavy");
        }
    }

    private void QueueLightAttack(string attack)
    {
        if (!attacking)
        {
            networkAnimator.SetTrigger(attack);
        }
        else
        {
            attackQueueTimestamp = Time.time; // Record when the input arrived
            attackQueue.Enqueue(attack);      // Always queue, validate later
        }
    }
    
    private void QueueHeavyAttack(string attack)
    {
        if (!attacking)
        {
            networkAnimator.SetTrigger(attack);
        }
        else
        {
            attackQueueTimestamp = Time.time;
            attackQueue.Enqueue(attack);
        }
    }
    public void OnAttackStart()
    {
        HandleRotation();
        actionReferences.move.action.Disable();
        rb.linearVelocity = Vector3.zero;
        attacking = true;
        currentChain++;
    }
    public void OnAttackEnd()
    {
        actionReferences.move.action.Enable();
        attacking = false;

        if (attackQueue.Count > 0)
        {
            float timeSinceQueued = Time.time - attackQueueTimestamp;
            float percentageOfBuffer = (timeSinceQueued / attackBufferTime) * 100f;
        
            if (timeSinceQueued <= attackBufferTime && currentChain <= maxChainLengthLight)
            {
                Debug.Log($"Attack queued valid! {timeSinceQueued:F2}s ago ({percentageOfBuffer:F0}% of buffer used)");
                string nextAttack = attackQueue.Dequeue();
                networkAnimator.SetTrigger(nextAttack);
            }
            else
            {
                Debug.Log($"Attack queued too early! {timeSinceQueued:F2}s ago, needed within {attackBufferTime:F2}s ({percentageOfBuffer:F0}% of buffer, {timeSinceQueued - attackBufferTime:F2}s too early)");
                currentChain = 0;
                attackQueue.Clear();
            }
        }

        attackQueueTimestamp = -1f;
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
