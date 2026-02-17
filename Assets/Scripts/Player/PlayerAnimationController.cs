using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private PlayerVerticalMovement verticalMovement;
    [SerializeField] private PlayerSprint playerSprint;

    [Header("Combat / Game")]
    [SerializeField] private PlayerCombatState combatState;   // eklendi
    [SerializeField] private HitResolver hitResolver;         // eklendi (topa değip gameover olursa)


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

    private bool _collapsed; // collapse bir kere olsun


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false; // Garanti olsun diye tekrar kapatıyoruz
    }

    private void OnEnable()
    {
        if (verticalMovement != null)
        {
            verticalMovement.OnJumpPerformed += HandleJump;
            verticalMovement.OnQuickDropPerformed += HandleQuickDrop;
            verticalMovement.OnGroundedStatusChanged += HandleGroundedChange;
        }
        if (combatState != null)
        {
            combatState.OnStagger += HandleTrip;
            combatState.OnFall += HandleCollapse;
        }

        if (hitResolver != null)
        {
            hitResolver.OnGameOver += HandleCollapse; // topa değme gibi durumlar
        }
    }

    private void OnDisable()
    {
        if (verticalMovement != null)
        {
            verticalMovement.OnJumpPerformed -= HandleJump;
            verticalMovement.OnQuickDropPerformed -= HandleQuickDrop;
            verticalMovement.OnGroundedStatusChanged -= HandleGroundedChange;
        }

        if (combatState != null)
        {
            combatState.OnStagger -= HandleTrip;
            combatState.OnFall -= HandleCollapse;
        }

        if (hitResolver != null)
        {
            hitResolver.OnGameOver -= HandleCollapse;
        }
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
    private void HandleGroundedChange(bool isGrounded) => _animator.SetBool(IsGroundedBool, isGrounded);

    private void HandleTrip()
    {
        if (_collapsed) return;
        _animator.SetTrigger(TripTrigger);
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
    }
}