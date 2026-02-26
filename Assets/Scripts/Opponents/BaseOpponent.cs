using System.Collections;
using UnityEngine;

public abstract class BaseOpponent : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float speed = 20f;

    [Header("Runtime")]
    [SerializeField] protected Transform targetPlayer;
    [SerializeField] protected Animator animator;
    protected static readonly int MirrorHash = Animator.StringToHash("Mirror");

    // ---- Animator Params (2 ayrı trigger) ----
    protected static readonly int AttackHash = Animator.StringToHash("DoAttack");
    protected static readonly int SlideHash = Animator.StringToHash("DoSlide");

    // State name hash (Animator state adın tam "Run" olmalı)
    protected static readonly int RunStateHash = Animator.StringToHash("Run");

    // Pooling reset için
    private bool _initRoutineRunning;

    [Header("Animation Speed Sync")]
    [SerializeField] private bool scaleAnimatorSpeed = true;

    [Tooltip("Anim speed = 1 olacağı referans dünya hızı (GameManager.CurrentSpeed).")]
    [SerializeField] private float worldSpeedReference = 10f;

    [Tooltip("Anim hızı için ekstra çarpan (göze göre).")]
    [SerializeField] private float animatorSpeedMultiplier = 1f;

    [Tooltip("Anim hızı clamp aralığı.")]
    [SerializeField] private float animatorSpeedMin = 0.85f;
    [SerializeField] private float animatorSpeedMax = 1.75f;


    [SerializeField] private bool syncMoveSpeedToWorld = true;

    [Tooltip("Dünya hızına göre çarpanlı sync için referans dünya hızı. Boş bırakmak için 0; otomatik InitialSpeed kullanılır.")]
    [SerializeField] private float worldSpeedReferenceOverride = 0f;

    [Tooltip("Rakip hızına ekstra çarpan (gözüne göre). 1 = tam sync.")]
    [SerializeField] private float moveSyncMultiplier = 1f;

    [Tooltip("Sync eğrisi. 1 = lineer (tam istediğin). 0.8 daha yumuşak, 1.2 daha agresif.")]
    [SerializeField] private float moveSyncExponent = 1f;

    protected virtual void Awake()
    {
        // Animator child'da ise otomatik bul
        if (!animator) animator = GetComponentInChildren<Animator>(true);

        // Güvenlik ayarları
        if (animator)
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.speed = 1f;
            animator.enabled = true;
        }
    }

    /// <summary>
    /// Pool'dan çıkınca çağrılır.
    /// </summary>
    public virtual void Initialize(Transform playerTransform)
    {
        targetPlayer = playerTransform;

        if (animator)
        {
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(SlideHash);

            bool mirror = Random.value < 0.5f;   // %50
            animator.SetBool(MirrorHash, mirror);

            if (!_initRoutineRunning)
                StartCoroutine(CoResetAnimatorNextFrame());
        }
    }

    private IEnumerator CoResetAnimatorNextFrame()
    {
        _initRoutineRunning = true;
        yield return null;

        if (!animator)
        {
            _initRoutineRunning = false;
            yield break;
        }

        // Hard reset
        animator.enabled = true;
        animator.speed = 1f;

        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(SlideHash);

        animator.Rebind();
        animator.Update(0f);

        // Run'a en baştan gir
        animator.Play(RunStateHash, 0, 0f);
        animator.Update(0f);

        _initRoutineRunning = false;
    }

    /// <summary>
    /// Pool'a geri dönmeden önce çağrılabilir.
    /// </summary>
    public virtual void OnDespawned()
    {
        targetPlayer = null;

        if (animator)
        {
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(SlideHash);
            animator.speed = 1f; // reset
        }
    }

    private void Update()
    {
        if (!targetPlayer) return;

        UpdateAnimatorSpeed(); // <-- EKLENDİ

        Move();
        CheckOutOfBounds();
    }


    private void UpdateAnimatorSpeed()
    {
        if (!scaleAnimatorSpeed || !animator) return;
        if (GameManager.Instance == null) return;

        // Dünya hızı (base * multiplier) = sprint dahil
        float world = GameManager.Instance.CurrentSpeed;

        // Referans dünya hızı: override varsa onu, yoksa InitialSpeed'i baz al
        float reference = worldSpeedReference > 0.01f
            ? worldSpeedReference
            : GameManager.Instance.InitialSpeed;

        if (reference <= 0.01f)
        {
            animator.speed = 1f;
            return;
        }

        // Dünya hız oranı: 1 = normal, 2 = iki kat, 4 = dört kat...
        float scale = world / reference;
        scale = Mathf.Max(0f, scale);

        // ---- YUMUŞATMA ----
        // 1x -> 1x
        // 4x -> 2x (aşırı hızlanmayı engeller)
        float eased = Mathf.Sqrt(scale);

        // İstersen daha da yumuşak/sert yapmak için:
        // eased = Mathf.Pow(scale, 0.5f);  // 0.5 = sqrt
        // eased = Mathf.Pow(scale, 0.4f);  // daha yumuşak
        // eased = Mathf.Pow(scale, 0.6f);  // daha agresif

        float animSpeed = eased * animatorSpeedMultiplier;

        animator.speed = Mathf.Clamp(animSpeed, animatorSpeedMin, animatorSpeedMax);
    }

    protected abstract void Move();

    protected virtual void CheckOutOfBounds()
    {
        if (transform.position.z < targetPlayer.position.z - 10f)
        {
            if (ObjectPooler.Instance)
                ObjectPooler.Instance.NotifyDespawned(this);

            OnDespawned();
            gameObject.SetActive(false);
        }
    }

    /*protected float GetCurrentSpeed()
    {
        float worldSpeed = GameManager.Instance ? GameManager.Instance.CurrentSpeed : 0f;
        return speed + (worldSpeed * 0.1f);

    }*/

    protected float GetCurrentSpeed()
    {
        if (!syncMoveSpeedToWorld || GameManager.Instance == null)
            return speed;

        float world = GameManager.Instance.CurrentSpeed;

        float reference = worldSpeedReferenceOverride > 0.01f
            ? worldSpeedReferenceOverride
            : GameManager.Instance.InitialSpeed;   // senin GameManager’da var

        if (reference <= 0.01f) return speed;

        // Dünya hız oranı
        float scale = world / reference;

        // Exponent ile ayarlanabilir (default 1 = tam lineer)
        scale = Mathf.Pow(scale, moveSyncExponent);

        return speed * scale * moveSyncMultiplier;
    }

    protected void TriggerAttack()
    {
        if (animator) animator.SetTrigger(AttackHash);
    }

    protected void TriggerSlide()
    {
        if (animator) animator.SetTrigger(SlideHash);
    }
}