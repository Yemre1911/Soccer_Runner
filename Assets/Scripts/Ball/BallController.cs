using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Setup")]
    public Transform PlayerSocket;

    private IBallState _currentState;

    private void Start()
    {
        SetupPhysics();
        ChangeState(new DribbleState());
    }

    private void Update()
    {
        _currentState?.OnUpdate(this, Time.deltaTime);
    }

    public void ChangeState(IBallState newState)
    {
        _currentState?.OnExit(this);
        _currentState = newState;
        _currentState?.OnEnter(this);
    }

    // --- PLAYER TARAFINDAN ÇAĞRILACAK METOTLAR ---

    public void OnPlayerJump()
    {
        // Eğer zaten havadaysak tekrar zıplama (Double jump yoksa)
        if (!(_currentState is BallJumpState))
        {
            ChangeState(new BallJumpState());
        }
    }

    public void OnPlayerQuickDrop()
    {
        // Eğer havadaysak state'e haber ver
        if (_currentState is BallJumpState jumpState)
        {
            jumpState.SetQuickDrop();
        }
    }

    private void SetupPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        GetComponent<SphereCollider>().isTrigger = true;
    }

    public void ReleaseBall()
    {
        ChangeState(null); // Update çalışmasını durdurur
        //Rigidbody rb = GetComponent<Rigidbody>();
        //rb.isKinematic = false;
        //rb.useGravity = true;
        //rb.velocity = new Vector3(0, 5f, 5f); // Hafif bir fırlama efekti
        //transform.SetParent(null); // Karakterden kopar
    }
}