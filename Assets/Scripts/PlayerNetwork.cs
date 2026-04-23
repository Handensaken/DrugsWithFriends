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
    private bool freeCamMovement, looking, attacking, isCameraLockedOn;
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
    
    private static class AnimationParameters
    {
        public const string LightAttack = "LightAttackBool";
        public const string HeavyAttack = "HeavyAttackBool";
        public const string ExitCombo = "ExitCombo";
        public const string Running = "Running";
        public const string TurnAround  = "TurnAround";
        public const string CombatX     = "combatX";
        public const string CombatY     = "combatY";
        public const string XInput      = "X-Input";
        public const string ZInput      = "Z-Input";
    }
    
    [SerializeField] private ActionReferences actionReferences;
    [SerializeField] private PauseEvent pauseEvent;
    [SerializeField] private ControlSchemeEvent controlSchemeEvent;
    [SerializeField] private PlayerGameSettings playerSettings;
    private Animator animator;
    private NetworkAnimator networkAnimator;
    private int enemyIndex, currentChain;
    [SerializeField] private List<Transform> enemiesInRange, enemiesOnScreen;
    private Queue<string> attackQueue;
    private PlayerInput playerInput;
    private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineCamera freeCam, lockOnCam;
    private Vector3 moveVector;
    private float attackQueueTimestamp = -1f;
    [SerializeField] private SphereCollider attackRangeCollider;
    [SerializeField] private BoxCollider attackHitboxCollider;
    [SerializeField] private SelectionHandler selectionHandler;

    protected override void OnValidate()
    {
        base.OnValidate();
        attackRangeCollider.radius = detectEnemiesRange;
    }

    private void Awake()
    {
        enemiesOnScreen = new List<Transform>();
        attackHitboxCollider.enabled = false;
        freeCamMovement = true;
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
            animator.SetFloat(AnimationParameters.XInput, 0);
            animator.SetFloat(AnimationParameters.ZInput, 0);
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
    
    private void SubscribeActions(bool register)
    {
        void Performed(InputActionReference r, Action<InputAction.CallbackContext> cb)
        {
            if (register)
            {
                r.action.performed += cb;
            }
            else
            {
                r.action.performed -= cb;
            }
        }
 
        void Canceled(InputActionReference r, Action<InputAction.CallbackContext> cb)
        {
            if (register)
            {
                r.action.canceled += cb;
            }
            else
            {
                r.action.canceled -= cb;
            }
        }
        Performed(actionReferences.move, Move);
        Canceled (actionReferences.move, Move);
 
        Performed(actionReferences.look, Look);
        Canceled (actionReferences.look, Look);
 
        Performed(actionReferences.toggleCameraFocus, ToggleCameraFocus);
        Performed(actionReferences.lightAttack, LightAttack);
        Performed(actionReferences.heavyAttack, HeavyAttack);

        if (register)
        {
            playerInput.onControlsChanged += ControlsChanged;
        }
        else
        {
            playerInput.onControlsChanged -= ControlsChanged;
        }
    }

    private void OnEnable()  => SubscribeActions(true);
    private void OnDisable() => SubscribeActions(false);

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
                if(other.transform == lockOnCam.LookAt) // Focus on player if exiting the area where enemy is focused 
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
        }
        
        if (moveVector.sqrMagnitude > 0.01f || !freeCamMovement)
        {
            HandleRotation();
        }
        CheckEnemiesOnScreen(); // Temporarily placed here
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
            animator.SetFloat(AnimationParameters.CombatX, 0);
            animator.SetFloat(AnimationParameters.CombatY, 0);
            animator.SetBool(AnimationParameters.Running, false);
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

    private (Vector3 forward, Vector3 right) SetCameraVectors(CinemachineCamera cam)
    {
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        return (forward, right);
    }

    private void FreeCamMovement()
    {
        Vector2 direction = actionReferences.move.action.ReadValue<Vector2>();
        if (direction.sqrMagnitude < 0.01) return;

        var (cameraForward, cameraRight) = SetCameraVectors(freeCam);

        moveVector = (cameraForward * direction.y + cameraRight * direction.x);
        rb.linearVelocity = moveVector * moveSpeed;
        animator.SetBool("Running", true);
    }

    private void TrackingObjectMovement()
    {
        Vector2 direction = actionReferences.move.action.ReadValue<Vector2>();
        if (direction.sqrMagnitude < 0.01) return;

        var (cameraForward, cameraRight) = SetCameraVectors(lockOnCam);

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

        enemyIndex = Mathf.Clamp(enemyIndex, 0, Mathf.Max(0, enemiesOnScreen.Count - 1));

        if (!isCameraLockedOn && enemiesOnScreen.Count > 0)
        {
            LockOnToEnemy(0);
        }
        else if (isCameraLockedOn && enemyIndex < enemiesOnScreen.Count - 1)
        {
            CycleTarget();
        }
        else
        {
            FocusOnPlayer();
        }
        
        SetCamera();
    }
    
    private void CycleTarget()
    {
        enemyIndex++;
        lockOnCam.LookAt = enemiesOnScreen[enemyIndex];
    }

    private void SetCamera()
    {
        freeCam.gameObject.SetActive(!isCameraLockedOn);
        lockOnCam.gameObject.SetActive(isCameraLockedOn);
    }

    private void FocusOnPlayer()
    {
        isCameraLockedOn = false;
        freeCam.transform.position = lockOnCam.transform.position;
        SetCamera();
        freeCamMovement = true;
        animator.SetLayerWeight(1, 0);
        actionReferences.look.action.Enable();
    }
    
    private void LockOnToEnemy(int index)
    {
        enemyIndex = index;
        isCameraLockedOn = true;
        freeCamMovement = false;
        animator.SetLayerWeight(1, 1);
        actionReferences.look.action.Disable();
        lockOnCam.LookAt = enemiesOnScreen[enemyIndex];
        lockOnCam.transform.position = freeCam.transform.position;
    }

    private void LightAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner || !context.performed) return;
        QueueAttack(AnimationParameters.LightAttack);
    }
    
    private void HeavyAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner || !context.performed) return;
        QueueAttack(AnimationParameters.HeavyAttack);
    }
    
    private void QueueAttack(string attack)
    {
        if (!attacking)
        {
            SetAnimatorBool(attack, true);
        }
        else
        {
            attackQueueTimestamp = Time.time;
            attackQueue.Enqueue(attack);
        }
    }
    public void OnAttackStart()
    {
        attackHitboxCollider.enabled = true;
        HandleRotation();
        SetAnimatorBool(AnimationParameters.ExitCombo, false);
        actionReferences.move.action.Disable();
        rb.linearVelocity = Vector3.zero;
        attacking = true;
        currentChain++;
    }
    public void OnAttackEnd()
    {
        attackHitboxCollider.enabled = false;
 
        float timeSinceQueued = Time.time - attackQueueTimestamp;
        bool withinBuffer = timeSinceQueued <= attackBufferTime;
        bool chainNotMaxed = currentChain < maxChainLengthLight;
 
        if (attackQueue.Count > 0 && withinBuffer && chainNotMaxed)
        {
            string nextAttack = attackQueue.Peek();
            int maxChain = nextAttack == AnimationParameters.LightAttack ? maxChainLengthLight : maxChainLengthHeavy;

            if (currentChain < maxChain)
            {
                attackQueue.Dequeue();
                float percentageOfBuffer = (timeSinceQueued / attackBufferTime) * 100f;
                Debug.Log($"Attack queued valid! {timeSinceQueued:F2}s ago ({percentageOfBuffer:F0}% of buffer used)");
                
                SetAnimatorBool(AnimationParameters.ExitCombo, false);
                SetAnimatorBool(nextAttack, true);
                return;
            }
        }
 
        attackQueueTimestamp = -1f;
        attackQueue.Clear();
        ExitCombo();
    }

    private void ExitCombo()
    {
        currentChain = 0;
        SetAnimatorBool(AnimationParameters.ExitCombo,   true);
        SetAnimatorBool(AnimationParameters.LightAttack, false);
        SetAnimatorBool(AnimationParameters.HeavyAttack, false);
        attacking = false;
        actionReferences.move.action.Enable();
    }
    
    private void SetAnimatorBool(string param, bool value)
    {
        animator.SetBool(param, value);
        ServerSetAnimatorBool(param, value);
    }
    
    [ServerRpc]
    private void ServerSetAnimatorBool(string param, bool value)
    {
        ObserversSetAnimatorBool(param, value);
    }

    [ObserversRpc(ExcludeOwner = true)] // Owner already set it locally
    private void ObserversSetAnimatorBool(string param, bool value)
    {
        animator.SetBool(param, value);
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
        enemiesOnScreen.Sort((a, b) => // Sort enemies by distance to center of screen
        {
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 vpA = Camera.main.WorldToViewportPoint(a.position);
            Vector2 vpB = Camera.main.WorldToViewportPoint(b.position);
            return Vector2.Distance(vpA, center).CompareTo(Vector2.Distance(vpB, center));
        });
    }
    
    private bool IsOnScreen(Transform target)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(target.position);

        // z > 0 means the target is in front of the camera
        return vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1;
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
