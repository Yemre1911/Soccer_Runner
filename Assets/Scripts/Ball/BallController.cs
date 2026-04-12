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
        EnterDribble();
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

    public void EnterIdle()
    {
        ChangeState(new BallIdleState());
    }

    public void EnterDribble()
    {
        ChangeState(new DribbleState());
    }

    public void SnapToSocketImmediate()
    {
        if (PlayerSocket == null) return;

        transform.position = PlayerSocket.position;
        transform.rotation = PlayerSocket.rotation;
    }

    public void OnPlayerJump()
    {
        if (!(_currentState is BallJumpState))
            ChangeState(new BallJumpState());
    }

    public void OnPlayerQuickDrop()
    {
        if (_currentState is BallJumpState jumpState)
            jumpState.SetQuickDrop();
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
        ChangeState(null);
    }
}