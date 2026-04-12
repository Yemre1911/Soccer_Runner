/*using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private PlayerVerticalMovement verticalMovement;
    [SerializeField] private PlayerLateralMovement lateralMovement;
    [SerializeField] private PlayerSprint playerSprint;

    [Header("Combat / Game")]
    [SerializeField] private PlayerCombatState combatState;   // eklendi
    [SerializeField] private HitResolver hitResolver;         // eklendi (topa değip gameover olursa)

    [Header("Effects")]
    [SerializeField] private SimpleCameraShake cameraShake;


    [Header("Hız Kalibrasyonu")]
    [Tooltip("Dünya hızını buna böleriz (Senin bulduğun 10 değeri)")]
    [SerializeField] private float speedDivisor = 10f;

    [Tooltip("Animasyon hızı bu değerin altına düşemez")]
    [SerializeField] private float minAnimSpeed = 0.7f;

    [Tooltip("Animasyon hızı bu değerin üstüne çıkamaz")]
    [SerializeField] private float maxAnimSpeed = 1.3f;

    [Tooltip("Başlangıç hızını dengelemek için çarpan (10/10 = 1 sonucunu 0.7 yapmak için)")]
    [SerializeField] private float calibration = 0.7f;

    private Animator _animator;

    // Hash ID'leri (Performans için)
    private static readonly int JumpTrigger = Animator.StringToHash("Jump");
    private static readonly int FastDropTrigger = Animator.StringToHash("FastDrop");
    private static readonly int IsGroundedBool = Animator.StringToHash("IsGrounded");
    private static readonly int RunSpeedFloat = Animator.StringToHash("RunSpeed");

    // yeni trigger hash'leri
    private static readonly int TripTrigger = Animator.StringToHash("Trip");
    private static readonly int CollapseTrigger = Animator.StringToHash("Collapse");

    private static readonly int SpinTrigger = Animator.StringToHash("Spin");
    private static readonly int SpinMirrorBool = Animator.StringToHash("SpinMirror");
    private bool _isGrounded;

    private bool _collapsed; // collapse bir kere olsun

    private Coroutine _subscribeRoutine; // instance yaratılana kadar almayı deneyen güvenli atama corutini


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false; // Garanti olsun diye tekrar kapatıyoruz
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
        {
            Debug.Log("[PAC] Subscribed to GameOver.OnGameLost"); // sonra sil
            GameOver.Instance.OnGameLost += HandleCollapse;
        }
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
            yield return null; // her frame kontrol

        GameOver.Instance.OnGameLost -= HandleCollapse; // double-subscribe engeli
        GameOver.Instance.OnGameLost += HandleCollapse;

        Debug.Log("[PAC] Subscribed to GameOver.OnGameLost (delayed)");
    }


    private void Update()
    {
        if (_collapsed) return; // collapse olduktan sonra run speed falan kurcalama

        UpdateAnimationSpeed();
    }

    private void UpdateAnimationSpeed()
    {
        // 1. Verileri al
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.CurrentSpeed : 10f;
        float sprintMultiplier = (playerSprint != null && playerSprint.IsSprinting) ? 1.5f : 1f;

        // 2. Ham hızı hesapla (Senin mantığın + Kalibrasyon)
        // Örnek: (10 / 10) * 1 * 0.7 = 0.7 (Başlangıç)
        // Örnek Sprint: (10 / 10) * 1.5 * 0.7 = 1.05 (Gayet akıcı)
        float rawSpeed = (worldSpeed / speedDivisor) * sprintMultiplier * calibration;

        // 3. Değeri 0.7 ile 1.3 arasına sıkıştır (Clamp)
        float finalSpeed = Mathf.Clamp(rawSpeed, minAnimSpeed, maxAnimSpeed);

        // 4. Animator'a gönder
        _animator.SetFloat(RunSpeedFloat, finalSpeed);
    }

    // --- Event Handlers ---
    private void HandleJump() => _animator.SetTrigger(JumpTrigger);
    private void HandleQuickDrop() => _animator.SetTrigger(FastDropTrigger);
    //private void HandleGroundedChange(bool isGrounded) => _animator.SetBool(IsGroundedBool, isGrounded);
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

        // her ihtimale karşı diğer triggerları resetlemek istersen:
        _animator.ResetTrigger(JumpTrigger);
        _animator.ResetTrigger(FastDropTrigger);
        _animator.ResetTrigger(TripTrigger);

        _animator.SetTrigger(CollapseTrigger);
        Debug.Log("Collapse Triggered");
    }

    private void HandleLaneChange(int direction)
    {
        TryPlaySpin(direction); // önceki yazdığım grounded/%50/mirror logic
    }

    public void TryPlaySpin(int direction)
    {
        if (_collapsed) return;
        if (!_isGrounded) return;                 // havadayken ASLA
        if (direction == 0) return;

        // Zaten Spin oynuyorsa tekrar tetikleme
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Spin")) return;

        // %50 şans
        if (Random.value > 0.2f) return;

        // sağdan sola giderken mirror
        bool mirror = direction < 0;
        _animator.SetBool(SpinMirrorBool, mirror);

        _animator.SetTrigger(SpinTrigger);
    }
}*/

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

    public void PlayShoot()
    {
        if (_collapsed) return;
        _animator.SetTrigger(ShootTrigger);
    }

    public void SetRunningAnimation(bool isRunning)
    {
        _animator.SetBool(IsRunningBool, isRunning);

        if (!isRunning)
            _animator.SetFloat(RunSpeedFloat, 0f);
    }
}