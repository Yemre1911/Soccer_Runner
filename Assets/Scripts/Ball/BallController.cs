/*using UnityEngine;

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
}*/

using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    [Header("Setup")]
    public Transform PlayerSocket;

    [Header("Socket Follow")]
    [SerializeField] private float socketSmoothTime = 0.03f;
    [SerializeField] private float socketMaxSpeed = 100f;
    [SerializeField] private float socketSnapDistance = 0.03f;

    [Header("Idle Spin")]
    [SerializeField] private float shotIdleSpinSpeed = 800f;

    private IBallState _currentState;

    public float SocketSmoothTime => socketSmoothTime;
    public float SocketMaxSpeed => socketMaxSpeed;
    public float SocketSnapDistance => socketSnapDistance;
    public float ShotIdleSpinSpeed => shotIdleSpinSpeed;

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
        ChangeState(new BallIdleState(false));
    }

    public void EnterIdle(bool keepSpin)
    {
        ChangeState(new BallIdleState(keepSpin));
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

    public void RotateVisualSpin(float xSpinSpeed, float zTiltSpeed, float deltaTime)
    {
        transform.Rotate(Vector3.right * xSpinSpeed * deltaTime, Space.World);
        transform.Rotate(Vector3.forward * zTiltSpeed * deltaTime, Space.World);
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