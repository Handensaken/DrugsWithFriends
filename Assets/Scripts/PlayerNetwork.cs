using System;
using System.Collections;
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
    [Range(0, 20f)] public float moveSpeed;
    [SerializeField, Range(0, 1f)] private float rotationSpeed;
    [SerializeField, ReadOnly] private float currentSpeed = 0f;
    private Vector2 rot;
    private bool freeCamMovement, looking, attacking, isCameraLockedOn, attackBuffered, isDead, dashing, transitioningFromLockOn;
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
        public InputActionReference move, look, dash, toggleCameraFocus, lightAttack, heavyAttack, spectateNext;
    }
    
    private static class AnimationParameters
    {
        public const string LightAttack = "lightAttack";
        public const string HeavyAttack = "heavyAttack";
        public const string ExitCombo = "ExitCombo";
        public const string Running = "Running";
        public const string TurnAround = "TurnAround";
        public const string DashForward = "Dash/Forward";
        public const string DashBackward = "Dash/Backward";
        public const string DashLeft = "Dash/Left";
        public const string DashRight = "Dash/Right";
        public const string CombatX = "combatX";
        public const string CombatY = "combatY";
        public const string XInput = "X-Input";
        public const string ZInput = "Z-Input";
        public const string Hurt = "Hurt";
        public const string Downed = "Downed";
        public const string Death = "Death";
    }
    
    [SerializeField] private ActionReferences actionReferences;
    [SerializeField] private ControlSchemeEvent controlSchemeEvent;
    [SerializeField] private PlayerGameSettings playerSettings;
    [SerializeField] private Animator animator;
    private NetworkAnimator networkAnimator;
    private int enemyIndex, currentChain;
    [SerializeField, ReadOnly] private List<Transform> enemiesInRange, enemiesOnScreen;
    private PlayerInput playerInput;
    private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineCamera freeCam, spectatorCamera, lockOnCam;
    private Vector3 moveVector;
    private float attackQueueTimestamp = -1f;
    [SerializeField] private SphereCollider attackRangeCollider;
    [SerializeField] private BoxCollider attackHitboxCollider;
    [SerializeField] private SelectionHandler selectionHandler;
    private string queuedAttack = "";
    [SerializeField, Range(0, 3f)] private float animationSpeed;
    
    [Serializable]
    struct DashParameters
    {
        [Range(0, 100f), Tooltip("")] public float dashForce; 
        [Range(0, 5f), Tooltip("")] public float dashCooldown;
        [Range(0, 2f), Tooltip("")] public float invincibilityDuration;
    }
    
    [SerializeField] private DashParameters dashParameters;
 
    [SerializeField, InspectorButton("PlayerDeath", "Kill Player")] private bool playerDeathButton;
    [SerializeField, InspectorButton("PlayerRespawn", "Respawn Player")] private bool playerRespawnButton;
    
    private int spectatorIndex = 0;
    private List<PlayerNetwork> alivePlayers = new List<PlayerNetwork>();
    [SerializeField] private GameObject spectatorCanvas;
    [SerializeField] private PauseMenu pauseMenu;
 
    protected override void OnValidate()
    {
        base.OnValidate();
        attackRangeCollider.radius = detectEnemiesRange;
        animator.speed = animationSpeed;
    }
 
    private void Awake()
    {
        enemiesOnScreen = new List<Transform>();
        attackHitboxCollider.enabled = false;
        freeCamMovement = true;
        currentChain = 0;
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
 
        animator.SetFloat(AnimationParameters.XInput, 0);
        animator.SetFloat(AnimationParameters.ZInput, 0);
        
        if (TryGetComponent(out NetworkAnimator nAnim))
        {
            networkAnimator = nAnim;
        }
        else
        {
            Debug.LogError("Couldn't get networkanimator");
        }
    }
 
    public override void OnStartClient()
    {
        base.OnStartClient();
        ControlsChanged(playerInput);
        if (!IsOwner)
        {
            foreach (var cam in GetComponentsInChildren<CinemachineCamera>())
                cam.enabled = false;
            foreach (var axis in GetComponentsInChildren<CinemachineInputAxisController>())
                axis.enabled = false;
            return;
        }
        SubscribeActions(true);
        Application.focusChanged += OnAppFocusChanged;
        actionReferences.spectateNext.action.Disable();
    
        Collider[] hits = Physics.OverlapSphere(transform.position, detectEnemiesRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy") && !enemiesInRange.Contains(hit.transform))
            {
                enemiesInRange.Add(hit.transform);
            }
        }
        
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
 
    public override void OnStopClient()
    {
        base.OnStopClient();
        SubscribeActions(false);
        Application.focusChanged -= OnAppFocusChanged;
    }
    
    private void OnAppFocusChanged(bool hasFocus)
    {
        if (!IsOwner || !isDead) return;
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
        Canceled(actionReferences.move, Move);
 
        Performed(actionReferences.look, Look);
        Canceled(actionReferences.look, Look);
        
        Performed(actionReferences.dash, Dash);
 
        Performed(actionReferences.toggleCameraFocus, ToggleCameraFocus);
        Performed(actionReferences.lightAttack, LightAttack);
        Performed(actionReferences.heavyAttack, HeavyAttack);
        
        Performed(actionReferences.spectateNext, SpectateNext);
 
        if (register)
        {
            playerInput.onControlsChanged += ControlsChanged;
        }
        else
        {
            playerInput.onControlsChanged -= ControlsChanged;
        }
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other.CompareTag("Enemy") && other.gameObject.activeSelf)
        {
            if (!enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Add(other.transform);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInRange.Contains(other.transform))
            {
                enemiesInRange.Remove(other.transform);
                if(other.transform == lockOnCam.LookAt)
                {
                    FocusOnPlayer();
                }
            }
        }
    }
 
    private void FixedUpdate()
    {
        if (!IsOwner) return;
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
            SetVelocity();
        }
        CheckEnemiesOnScreen();
    }
 
    public void PlayerDeath()
    {
        if (!IsOwner) return;
        isDead = true;
        if (playerInput.currentActionMap.name == "UI")
        {
            pauseMenu.previousActionMap = "Spectator";
        }

        else
        {
            playerInput.SwitchCurrentActionMap("Spectator");
        }
        
        spectatorCanvas.SetActive(true);
        lockOnCam.gameObject.SetActive(false);
        freeCam.gameObject.SetActive(false);
        spectatorCamera.gameObject.SetActive(true);
        
        networkAnimator.SetTrigger(AnimationParameters.Downed);
 
        RefreshAlivePlayers();
        if (alivePlayers.Count > 0)
        {
            spectatorIndex = 0;
            AttachSpectatorCamToPlayer(alivePlayers[spectatorIndex]);
        }
 
        Debug.Log("Player has died.");
    }
    
    public void PlayerRespawn()
    {
        if (!IsOwner) return;
        isDead = false;
        
        animator.SetBool(AnimationParameters.Death, false);
 
        spectatorCamera.gameObject.SetActive(false);
        playerInput.SwitchCurrentActionMap("Player");
        spectatorCanvas.SetActive(false);
        isCameraLockedOn = false;
        freeCamMovement = true;
        freeCam.gameObject.SetActive(true);
        lockOnCam.gameObject.SetActive(false);
 
        Debug.Log("Player has respawned.");
    }
    
    private void RefreshAlivePlayers()
    {
        alivePlayers.Clear();
        foreach (var p in FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None))
        {
            if (p != this && !p.isDead)
                alivePlayers.Add(p);
        }
    }
 
    private void AttachSpectatorCamToPlayer(PlayerNetwork target)
    {
        spectatorCamera.Follow = target.freeCam.Follow;
        spectatorCamera.LookAt = target.freeCam.LookAt;
    }
    
    private void SpectateNext(InputAction.CallbackContext context)
    {
        if (!IsOwner || !isDead) return;
        RefreshAlivePlayers();
        if (alivePlayers.Count == 0) return;
        spectatorIndex = (spectatorIndex + 1) % alivePlayers.Count;
        AttachSpectatorCamToPlayer(alivePlayers[spectatorIndex]);
    }
 
    private void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if(isDead) return;
        if (context.performed)
        {
            SetVelocity();
            currentSpeed = rb.linearVelocity.magnitude;
        }
        else if (context.canceled)
        {
            rb.linearVelocity = new Vector3(0, 0, 0);
            animator.SetFloat(AnimationParameters.CombatX, 0);
            animator.SetFloat(AnimationParameters.CombatY, 0);
            animator.SetBool(AnimationParameters.Running, false);
            currentSpeed = rb.linearVelocity.magnitude;
        }
    }
 
    private void Dash(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if(isDead) return;
        if (freeCamMovement)
        {
            Vector3 dashDirection;
            dashDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
            networkAnimator.SetTrigger(AnimationParameters.DashForward);
            rb.AddForce(dashParameters.dashForce * dashDirection, ForceMode.Impulse);
        }
        else
        {
            Vector2 dashDirection = actionReferences.move.action.ReadValue<Vector2>();

            if (dashDirection.sqrMagnitude < 0.01f)
            {
                networkAnimator.SetTrigger(AnimationParameters.DashBackward);
                dashDirection = new Vector3(-transform.forward.x, 0, -transform.forward.z);
                rb.AddForce(dashParameters.dashForce * dashDirection, ForceMode.Impulse);
                return;
            }

            if (dashDirection.x > 0f)
            {
                networkAnimator.SetTrigger(AnimationParameters.DashRight);
                rb.AddForce(dashParameters.dashForce * dashDirection, ForceMode.Impulse);
                return;
            }
            
            if(dashDirection.x < 0f)
            {
                networkAnimator.SetTrigger(AnimationParameters.DashLeft);
                rb.AddForce(dashParameters.dashForce * dashDirection, ForceMode.Impulse);
                return;
            }
            
            if(dashDirection.y > 0f)
            {
                networkAnimator.SetTrigger(AnimationParameters.DashForward);
                rb.AddForce(dashParameters.dashForce * dashDirection, ForceMode.Impulse);
                return;
            }
            
            if(dashDirection.y < 0f)
            {
                networkAnimator.SetTrigger(AnimationParameters.DashBackward);
                rb.AddForce(dashParameters.dashForce * dashDirection, ForceMode.Impulse);
            }
        }
    }

    public void StartDash()
    {
        dashing = true;
    }
    
    public void EndDash()
    {
        dashing = false;
    }
 
    private void HandleRotation()
    {
        if (transitioningFromLockOn) return;
        Vector3 rotationTarget = new Vector3();
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
            //networkAnimator.SetTrigger(AnimationParameters.TurnAround);
        }
            
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed));
    }
 
    private void SetVelocity()
    {
        if(isDead || dashing) return;
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
        animator.SetBool(AnimationParameters.Running, true);
    }
 
    private void TrackingObjectMovement()
    {
        Vector2 direction = actionReferences.move.action.ReadValue<Vector2>();
        if (direction.sqrMagnitude < 0.01) return;
 
        var (cameraForward, cameraRight) = SetCameraVectors(lockOnCam);
 
        moveVector = (cameraForward * direction.y + cameraRight * direction.x);
        rb.linearVelocity = moveVector * moveSpeed;
        animator.SetFloat(AnimationParameters.CombatY, direction.y);
        animator.SetFloat(AnimationParameters.CombatX, direction.x);
    }
 
    private void Look(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if(isDead) return;
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
        if(isDead) return;
 
        enemyIndex = Mathf.Clamp(enemyIndex, 0, Mathf.Max(0, enemiesOnScreen.Count - 1));
        
        if (!isCameraLockedOn && enemiesOnScreen.Count == 0) return;
 
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
        enemyIndex = 0;

        moveVector = transform.forward;

        freeCamMovement = true;
        animator.SetLayerWeight(1, 0);
        actionReferences.look.action.Enable();

        //freeCam.PreviousStateIsValid = false;

        transitioningFromLockOn = true;
        
        freeCam.ForceCameraPosition(
            lockOnCam.State.GetFinalPosition(),
            lockOnCam.State.GetFinalOrientation());

        SetCamera();
        
        StartCoroutine(EndTransition());
        
        Debug.Log(Vector3.Distance(
            freeCam.State.GetFinalPosition(),
            lockOnCam.State.GetFinalPosition()));
    }
    
    private IEnumerator EndTransition()
    {
        yield return new WaitForSeconds(0.5f);
        transitioningFromLockOn = false;
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
        lockOnCam.transform.rotation = freeCam.transform.rotation; 
    }
 
    public void EnableHitBox()
    {
        attackHitboxCollider.enabled = true;
    }
 
    public void DisableHitBox()
    {
        attackHitboxCollider.enabled = false;
    }
 
    private void LightAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner || isDead) return;
        if (!attacking)
        {
            networkAnimator.SetTrigger(AnimationParameters.LightAttack);
        }
        else
        {
            attackBuffered = true;
            attackQueueTimestamp = Time.time;
            queuedAttack = AnimationParameters.LightAttack;
        }
    }

    private void HeavyAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner || isDead) return;
        if (!attacking)
        {
            networkAnimator.SetTrigger(AnimationParameters.HeavyAttack);
        }
        else
        {
            attackBuffered = true;
            attackQueueTimestamp = Time.time;
            queuedAttack = AnimationParameters.HeavyAttack;
        }
    }

    public void OnAttackStart()
    {
        if (currentChain == 0)
            currentChain = 1;

        attackBuffered = false;
        animator.SetBool(AnimationParameters.ExitCombo, false);
        actionReferences.move.action.Disable();
        rb.linearVelocity = Vector3.zero;
        attacking = true;
    }
 
    public void OnAttackEnd()
    {
        float timeSinceQueued = Time.time - attackQueueTimestamp;
        bool withinBuffer = attackBuffered && timeSinceQueued <= attackBufferTime;

        Debug.Log($"[OnAttackEnd] attackBuffered={attackBuffered}, timeSinceQueued={timeSinceQueued:F3}, attackBufferTime={attackBufferTime}, queuedAttack={queuedAttack}, currentChain={currentChain}, maxChainLight={maxChainLengthLight}, maxChainHeavy={maxChainLengthHeavy}, withinBuffer={withinBuffer}");

        if (withinBuffer && queuedAttack == AnimationParameters.LightAttack && currentChain < maxChainLengthLight)
        {
            currentChain++;
            attackBuffered = false;
            networkAnimator.SetTrigger(queuedAttack);
            attacking = false;
            return;
        }

        if (withinBuffer && queuedAttack == AnimationParameters.HeavyAttack && currentChain < maxChainLengthHeavy)
        {
            currentChain++;
            attackBuffered = false;
            networkAnimator.SetTrigger(queuedAttack);
            attacking = false;
            return;
        }

        if (attackBuffered)
            Debug.LogWarning($"[OnAttackEnd] Buffer NOT consumed. withinBuffer={withinBuffer}, timeSinceQueued={timeSinceQueued:F3}, queuedAttack={queuedAttack}, currentChain={currentChain}, maxChainLight={maxChainLengthLight}, maxChainHeavy={maxChainLengthHeavy}");

        if (queuedAttack == AnimationParameters.LightAttack && currentChain < maxChainLengthLight)
        {
            StartCoroutine(EnableAttackAfterDelay(lightChainAttackCooldown));
            return;
        }

        if (queuedAttack == AnimationParameters.HeavyAttack && currentChain < maxChainLengthHeavy)
        {
            StartCoroutine(EnableAttackAfterDelay(heavyChainAttackCooldown));
            return;
        }
        
        ExitCombo();
    }

    private void ExitCombo()
    {
        currentChain = 0;
        attacking = false;
        actionReferences.move.action.Enable();
        animator.SetBool(AnimationParameters.ExitCombo, true);
    }
    
    private IEnumerator EnableAttackAfterDelay(float delay)
    {
        currentChain = 0;
        animator.SetBool(AnimationParameters.ExitCombo, true);
        actionReferences.move.action.Enable();
        yield return new WaitForSeconds(delay);
        attacking = false;
    }
 
    private void CheckEnemiesOnScreen()
    {
        enemiesOnScreen = new List<Transform>();
        foreach (Transform enemy in enemiesInRange)
        {
            bool onScreen = IsOnScreen(enemy);
 
            if (onScreen)
            {
                enemiesOnScreen.Add(enemy);
            }
        }
        enemiesOnScreen.Sort((a, b) =>
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
        //return vp.z > 0 && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1;
        return true;
    }
    
    private void ControlsChanged(PlayerInput input)
    {
        if (controlSchemeEvent.currentControlScheme is null) return;
        controlSchemeEvent.currentControlScheme = input.currentControlScheme.ToLower();
        if(!controlSchemeEvent.currentControlScheme.Contains("keyboard"))
        {
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