using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Setup")]
    public Transform PlayerSocket; // Karakterin önündeki boş obje (Sürükle-Bırak)

    private IBallState _currentState;

    private void Start()
    {
        SetupPhysics();
        // Oyuna "Dribble" (Sürme) moduyla başla
        ChangeState(new DribbleState());
    }

    private void Update()
    {
        _currentState?.OnUpdate(this, Time.deltaTime);
    }

    // Gelecekte Şut veya Pas eklendiğinde burayı kullanacağız
    public void ChangeState(IBallState newState)
    {
        _currentState?.OnExit(this);
        _currentState = newState;
        _currentState?.OnEnter(this);
    }

    // Performans için Fizik Ayarları
    private void SetupPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Fiziği kapattık, kodla yönetiyoruz
        rb.useGravity = false;
        
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true; // Çarpışmaları sadece "Bilgi" olarak alacağız
    }

    // Çarpışma Mantığı (İlerisi için hazırlık)
   /* private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Rakip topa değdi!");
            // İleride buraya: ChangeState(new LostBallState());
        }
        else if (other.CompareTag("OutOfBounds"))
        {
            Debug.Log("Top dışarı çıktı!");
        }
    }*/
}