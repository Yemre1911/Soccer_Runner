/*using UnityEngine;

// Her türlü rakibin türeyeceği soyut sınıf
public abstract class BaseOpponent : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float speed = 20f;

    [Header("Runtime")]
    [SerializeField] protected Transform targetPlayer;
    [SerializeField] protected Animator animator;

    protected static readonly int ActionHash = Animator.StringToHash("DoAction");
    protected static readonly int RunHash = Animator.StringToHash("Run");

    // Pool'dan çıkınca çağrılır
    public virtual void Initialize(Transform playerTransform)
    {
        targetPlayer = playerTransform;
        // pooling: gerekli state resetlerini burada yapabilirsin
        // örn: internal timer, saldırı state vs.

        if (animator) animator.Play(RunHash);
        Debug.Log("Run Animasyonu Başladı");
    }

    // Pool'a geri dönmeden hemen önce (opsiyonel)
    public virtual void OnDespawned()
    {
        targetPlayer = null;
    }

    protected abstract void Move();

    private void Update()
    {
        if (!targetPlayer) return;

        Move();
        CheckOutOfBounds();
    }

    protected virtual void CheckOutOfBounds()
    {
        if (transform.position.z < targetPlayer.position.z - 10f)
        {
            // listeyi güncelle
            if (ObjectPooler.Instance)
                ObjectPooler.Instance.NotifyDespawned(this);

            gameObject.SetActive(false);
            OnDespawned();
        }
    }

    protected float GetCurrentSpeed()
    {
        // Eğer GameManager yoksa (test sahnesi vs.) default speed kullan
        float globalSpeed = GameManager.Instance ? GameManager.Instance._baseSpeed : 0f;

        // Senin formülün: Kendi hızı + (Oyun Hızının 10'da 1'i)
        return speed + (globalSpeed * 0.1f);
    }
}*/

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

        // pooling: action state resetleri vb. child sınıflar yapabilir
        // Animator reseti burada tek yerden yapıyoruz:
        if (animator)
        {
            // Aynı obje tekrar spawn oluyorsa eski triggerlar kalmasın
            animator.ResetTrigger(AttackHash);
            animator.ResetTrigger(SlideHash);

            bool mirror = Random.value < 0.5f;   // %50
            animator.SetBool(MirrorHash, mirror);

            // En sağlam yöntem: 1 frame bekle, sonra Rebind/Play
            // (Enable frame’i stabil otursun diye)
            if (!_initRoutineRunning)
                StartCoroutine(CoResetAnimatorNextFrame());
        }
    }

    private IEnumerator CoResetAnimatorNextFrame()
    {
        _initRoutineRunning = true;

        // kritik: 1 frame bekle
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
        }
    }

    private void Update()
    {
        if (!targetPlayer) return;

        Move();
        CheckOutOfBounds();
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

    protected float GetCurrentSpeed()
    {
        float globalSpeed = GameManager.Instance ? GameManager.Instance._baseSpeed : 0f;
        return speed + (globalSpeed * 0.1f);
    }

    // Helper'lar: child sınıflar buradan aksiyon tetikler
    protected void TriggerAttack()
    {
        if (animator) animator.SetTrigger(AttackHash);
    }

    protected void TriggerSlide()
    {
        if (animator) animator.SetTrigger(SlideHash);
    }
}
