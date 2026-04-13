using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private PlayerVerticalMovement verticalMovement;
    [SerializeField] private PlayerLateralMovement lateralMovement;
    [SerializeField] private PlayerSprint playerSprint;

    [Header("Combat / Game")]
    [SerializeField] private PlayerCombatState combatState;
    [SerializeField] private HitResolver hitResolver;

    [Header("Effects")]
    [SerializeField] private SimpleCameraShake cameraShake;

    [Header("Hız Kalibrasyonu")]
    [SerializeField] private float speedDivisor = 10f;
    [SerializeField] private float minAnimSpeed = 0.7f;
    [SerializeField] private float maxAnimSpeed = 1.3f;
    [SerializeField] private float calibration = 0.7f;

    [Header("Idle")]
    [SerializeField] private float idleSpeedThreshold = 0.01f;

    private Animator _animator;

    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int FastDropTrigger = Animator.StringToHash("FastDrop");
    private static readonly int IsGroundedBool = Animator.StringToHash("IsGrounded");
    private static readonly int RunSpeedFloat = Animator.StringToHash("RunSpeed");
    private static readonly int TripTrigger = Animator.StringToHash("Trip");
    private static readonly int CollapseTrigger = Animator.StringToHash("Collapse");
    private static readonly int SpinTrigger = Animator.StringToHash("Spin");
    private static readonly int SpinMirrorBool = Animator.StringToHash("SpinMirror");
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");
    private static readonly int ShootMirrorBool = Animator.StringToHash("ShootMirror");

    private bool _isGrounded;
    private bool _collapsed;
    private Coroutine _subscribeRoutine;

    private static readonly int IsRunningBool = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
    }

    private void OnEnable()
    {
        if (_subscribeRoutine != null) StopCoroutine(_subscribeRoutine);
        _subscribeRoutine = StartCoroutine(SubscribeGameOverWhenReady());

        if (verticalMovement != null)
        {
            verticalMovement.OnJumpPerformed += HandleJump;
            verticalMovement.OnQuickDropPerformed += HandleQuickDrop;
            verticalMovement.OnGroundedStatusChanged += HandleGroundedChange;
        }

        if (lateralMovement != null)
            lateralMovement.OnLaneChangeRequested += HandleLaneChange;

        if (combatState != null)
        {
            combatState.OnStagger += HandleTrip;
            combatState.OnFall += HandleCollapse;
        }

        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost += HandleCollapse;
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }

        if (verticalMovement != null)
        {
            verticalMovement.OnJumpPerformed -= HandleJump;
            verticalMovement.OnQuickDropPerformed -= HandleQuickDrop;
            verticalMovement.OnGroundedStatusChanged -= HandleGroundedChange;
        }

        if (lateralMovement != null)
            lateralMovement.OnLaneChangeRequested -= HandleLaneChange;

        if (combatState != null)
        {
            combatState.OnStagger -= HandleTrip;
            combatState.OnFall -= HandleCollapse;
        }

        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost -= HandleCollapse;
    }

    private IEnumerator SubscribeGameOverWhenReady()
    {
        while (GameOver.Instance == null)
            yield return null;

        GameOver.Instance.OnGameLost -= HandleCollapse;
        GameOver.Instance.OnGameLost += HandleCollapse;
    }

    private void Update()
    {
        if (_collapsed) return;
        UpdateAnimationSpeed();
    }

    /*private void UpdateAnimationSpeed()
    {
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.CurrentSpeed : 10f;

        if (worldSpeed <= idleSpeedThreshold)
        {
            _animator.SetFloat(RunSpeedFloat, 0f);
            return;
        }

        float sprintMultiplier = (playerSprint != null && playerSprint.IsSprinting) ? 1.5f : 1f;
        float rawSpeed = (worldSpeed / speedDivisor) * sprintMultiplier * calibration;
        float finalSpeed = Mathf.Clamp(rawSpeed, minAnimSpeed, maxAnimSpeed);

        _animator.SetFloat(RunSpeedFloat, finalSpeed);
    }*/

    private void UpdateAnimationSpeed()
    {
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.CurrentSpeed : 0f;

        bool isRunning = worldSpeed > idleSpeedThreshold;
        _animator.SetBool(IsRunningBool, isRunning);

        if (!isRunning)
        {
            _animator.SetFloat(RunSpeedFloat, 0f);
            return;
        }

        float sprintMultiplier = (playerSprint != null && playerSprint.IsSprinting) ? 1.5f : 1f;
        float rawSpeed = (worldSpeed / speedDivisor) * sprintMultiplier * calibration;
        float finalSpeed = Mathf.Clamp(rawSpeed, minAnimSpeed, maxAnimSpeed);

        _animator.SetFloat(RunSpeedFloat, finalSpeed);
    }

    private void HandleJump() => _animator.SetTrigger(JumpTrigger);
    private void HandleQuickDrop() => _animator.SetTrigger(FastDropTrigger);

    private void HandleGroundedChange(bool isGrounded)
    {
        _isGrounded = isGrounded;
        _animator.SetBool(IsGroundedBool, isGrounded);
    }

    private void HandleTrip()
    {
        if (_collapsed) return;
        _animator.SetTrigger(TripTrigger);
        if (cameraShake != null) cameraShake.TriggerShake(0.3f, 0.3f);
    }

    private void HandleCollapse()
    {
        if (_collapsed) return;
        _collapsed = true;

        _animator.ResetTrigger(JumpTrigger);
        _animator.ResetTrigger(FastDropTrigger);
        _animator.ResetTrigger(TripTrigger);

        _animator.SetTrigger(CollapseTrigger);
    }

    private void HandleLaneChange(int direction)
    {
        TryPlaySpin(direction);
    }

    public void TryPlaySpin(int direction)
    {
        if (_collapsed) return;
        if (!_isGrounded) return;
        if (direction == 0) return;

        var state = _animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Spin")) return;

        if (Random.value > 0.2f) return;

        bool mirror = direction < 0;
        _animator.SetBool(SpinMirrorBool, mirror);
        _animator.SetTrigger(SpinTrigger);
    }

    public void PlayShoot(bool mirror)
    {
        if (_collapsed) return;

        _animator.SetBool(ShootMirrorBool, mirror);
        _animator.SetTrigger(ShootTrigger);
    }

    public void SetRunningAnimation(bool isRunning)
    {
        _animator.SetBool(IsRunningBool, isRunning);

        if (!isRunning)
            _animator.SetFloat(RunSpeedFloat, 0f);
    }
}