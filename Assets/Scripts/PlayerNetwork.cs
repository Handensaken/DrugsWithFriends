using System;
using System.Collections.Generic;
using FishNet.Component.Animating;
using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerInput))]
public class PlayerNetwork : NetworkBehaviour
{
    public float moveSpeed;
    [SerializeField, Range(0, 1f)] private float rotationSpeed;
    private Vector2 rot;
    [SerializeField] private bool freeCamMovement;
    private bool looking, attacking;
    private Rigidbody rb;
    [SerializeField, Range(0, 10f)] private float range;
    [SerializeField, Range(0, 10f)] private float detectEnemiesRange;
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
    private List<Transform> enemiesInRange, enemiesOnScreen;
    private Queue<string> attackQueue;
    private PlayerInput playerInput;
    private CinemachineCamera cinemachineCamera;
    private Vector3 moveVector;
    private float attackQueueTimestamp = -1f;
    [SerializeField] private SphereCollider attackRangeCollider;
    
    [SerializeField] private SelectionHandler selectionHandler;

    protected override void OnValidate()
    {
        base.OnValidate();
        attackRangeCollider.radius = detectEnemiesRange;
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Add(other.transform);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Remove(other.transform);
                if(other.transform == cinemachineCamera.LookAt)
                {
                    FocusOnPlayer();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        //Debug.Log(currentChain);
        if (looking || !freeCamMovement)
        {
            if(rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                SetVelocity();
            }
            CheckEnemiesOnScreen(); // Temporarily placed here
        }
        
        if (moveVector.sqrMagnitude > 0.01f || !freeCamMovement)
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
            animator.SetFloat("combatX", 0);
            animator.SetFloat("combatY", 0);
            animator.SetBool("Running", false);
        }
    }

    private void HandleRotation()
    {
        Vector3 rotationTarget;
        Transform currentTarget = null;
        if (!freeCamMovement)
        {
            if (enemiesOnScreen.Count > 0)
            {
                currentTarget = enemiesOnScreen[0];
            }
            if (currentTarget != null)
            {
                Vector3 toEnemy = currentTarget.position - transform.position;
                rotationTarget = new Vector3(toEnemy.x, 0f, toEnemy.z).normalized;
            }
            else
            {
                rotationTarget = new Vector3(cinemachineCamera.transform.forward.x, 0f, cinemachineCamera.transform.forward.z).normalized;
            }
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

        if (angleDifference > 120)
        {
            networkAnimator.SetTrigger("TurnAround");
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
        animator.SetBool("Running", true);
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
        animator.SetFloat("combatY", direction.y);
        animator.SetFloat("combatX", direction.x);
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
        if(cameraIndex == 0 && enemiesOnScreen.Count > 0)
        {
            enemyIndex = 0;
            freeCamMovement = false;
            animator.SetLayerWeight(1, 1);
            cinemachineCamera.LookAt = enemiesOnScreen[enemyIndex];
            actionReferences.look.action.Disable();
            cameraIndex = 1;
        }
        else if (enemyIndex < enemiesOnScreen.Count - 1  && enemiesOnScreen.Count > 0)
        {
            enemyIndex++;
            cinemachineCamera.LookAt = enemiesOnScreen[enemyIndex];
        }
        else
        {
            FocusOnPlayer();
        }
    }

    private void FocusOnPlayer()
    {
        cameraIndex = 0;
        freeCamMovement = true;
        animator.SetLayerWeight(1, 0);
        actionReferences.look.action.Enable();
        cinemachineCamera.LookAt = cinemachineCamera.Target.TrackingTarget;
    }

    private void LightAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed)
        {
            QueueLightAttack("LightAttackBool");
        }
    }
    
    private void HeavyAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (context.performed)
        {
            QueueHeavyAttack("HeavyAttackBool");
        }
    }

    private void QueueLightAttack(string attack)
    {
        if (!attacking) // If not currently attacking, perform the attack immediately
        {
            animator.SetBool(attack, true);
        }
        else
        {
            attackQueueTimestamp = Time.time;
            attackQueue.Enqueue(attack);
        }
    }
    
    private void QueueHeavyAttack(string attack)
    {
        if (!attacking)
        {
            animator.SetBool(attack, true);
        }
        else
        {
            attackQueueTimestamp = Time.time;
            attackQueue.Enqueue(attack);
        }
    }
    public void OnAttackStart()
    {
        animator.SetBool("LightAttackBool", false);
        animator.SetBool("HeavyAttackBool", false);
        HandleRotation();
        animator.SetBool("ExitCombo", false);
        actionReferences.move.action.Disable();
        rb.linearVelocity = Vector3.zero;
        attacking = true;
        currentChain++;
    }
    public void OnAttackEnd()
    {
        if (attackQueue.Count > 0)
        {
            float timeSinceQueued = Time.time - attackQueueTimestamp;
            float percentageOfBuffer = (timeSinceQueued / attackBufferTime) * 100f;
        
            if (timeSinceQueued <= attackBufferTime && currentChain < maxChainLengthLight)
            {
                Debug.Log($"Attack queued valid! {timeSinceQueued:F2}s ago ({percentageOfBuffer:F0}% of buffer used)");
                string nextAttack = attackQueue.Dequeue();
                Debug.Log("Performing queued attack: " + nextAttack);
                animator.SetBool(nextAttack, true);
            }
            else
            {
                Debug.Log($"Attack queued too early! {timeSinceQueued:F2}s ago, needed within {attackBufferTime:F2}s ({percentageOfBuffer:F0}% of buffer, {timeSinceQueued - attackBufferTime:F2}s too early)");
                currentChain = 0;
                attackQueue.Clear();
                animator.SetBool("ExitCombo", true);
                attacking = false;
                actionReferences.move.action.Enable();
            }
        }
        else
        {
            animator.SetBool("ExitCombo", true);
            attacking = false;
            actionReferences.move.action.Enable();
        }

        attackQueueTimestamp = -1f;
    }

    private void CheckEnemiesOnScreen()
    {
        enemiesOnScreen = new List<Transform>();
        foreach (Transform enemy in enemiesInRange)
        {
            bool onScreen = IsOnScreen(enemy);

            if (onScreen)
            {
                Debug.Log($"{enemy.name} is visible and within range!");
                enemiesOnScreen.Add(enemy);
            }
        }
    }
    
    private bool IsOnScreen(Transform target)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(target.position);

        // z > 0 means the target is in front of the camera
        return vp.z > 0
               && vp.x > 0 && vp.x < 1
               && vp.y > 0 && vp.y < 1;
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
