using UnityEngine;

// Her türlü rakibin türeyeceği soyut sınıf
public abstract class BaseOpponent : MonoBehaviour
{
    [Header("Base Settings")]
    protected float speed = 20f;

    [Header("Runtime")]
    [SerializeField] protected Transform targetPlayer;

    // Pool'dan çıkınca çağrılır
    public virtual void Initialize(Transform playerTransform)
    {
        targetPlayer = playerTransform;
        // pooling: gerekli state resetlerini burada yapabilirsin
        // örn: internal timer, saldırı state vs.
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
        // Rakip, player'ın çok arkasında kalınca pool'a dön
        if (transform.position.z < targetPlayer.position.z - 10f)
        {
            gameObject.SetActive(false);
            OnDespawned();
        }
    }

    protected float GetCurrentSpeed()
    {
        // Eğer GameManager yoksa (test sahnesi vs.) default speed kullan
        float globalSpeed = GameManager.Instance ? GameManager.Instance._baseSpeed : 0f;

        // Senin formülün: Kendi hızı + (Oyun Hızının 10'da 1'i)
        Debug.Log("HIZ: " + speed + (globalSpeed * 0.1f));
        return speed + (globalSpeed * 0.1f);
    }
}