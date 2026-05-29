/*using UnityEngine;

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

    [Header("Final Impact References")]
    [Tooltip("Clean catch durumlarında topun takip edeceği nokta. Keeper el/göğüs rig altında olabilir.")]
    [SerializeField] private Transform glovePoint;

    [Tooltip("Deflect/save rebound yönü için oyuncu veya kamera/player tarafı referansı.")]
    [SerializeField] private Transform playerReference;

    [Header("Final Impact - Goal Continue")]
    [SerializeField] private float goalContinueDistance = 7f;
    [SerializeField] private float goalContinueDuration = 0.35f;
    [SerializeField] private float goalContinueArcHeight = 0.6f;

    [Header("Final Impact - Goal Drop")]
    [Tooltip("Gol sonrası topun ineceği global zemin Y pozisyonu.")]
    [SerializeField] private float goalGroundY = -0.52f;

    [Tooltip("Top kalenin içine girdikten sonra zemine kaç saniyede insin?")]
    [SerializeField] private float goalDropDuration = 0.55f;

    [Tooltip("Yere inerken spin başlangıç çarpanı. 1 = mevcut spinSpeed, 0.5 = yarısı.")]
    [SerializeField] private float goalDropStartSpinMultiplier = 0.65f;

    [Tooltip("İniş sırasında çok hafif ekstra ileri kayma. Genelde 0 bırakabilirsin.")]
    [SerializeField] private float goalDropForwardSlideDistance = 0f;

    [Header("Final Impact - Out / Miss")]
    [SerializeField] private float outContinueDistance = 6f;
    [SerializeField] private float outContinueDuration = 0.4f;
    [SerializeField] private float outContinueArcHeight = 0.35f;

    [Header("Final Impact - Catch")]
    [SerializeField] private float catchSnapDuration = 0.08f;
    [SerializeField] private float catchFollowSpinSpeed = 450f;

    [Header("Final Impact - Deflect")]
    [SerializeField] private float deflectDistance = 8f;
    [SerializeField] private float deflectDuration = 0.45f;
    [SerializeField] private float deflectArcHeight = 1.2f;
    [SerializeField] private float deflectSideOffset = 1.5f;
    [SerializeField] private float deflectSpinSpeed = 1800f;

    [Header("Final Impact - Timing")]
    [SerializeField] private bool finalImpactUseUnscaledTime = true;

    [Header("Shot Result")]
    [SerializeField] private ShotPostResultAnimationDirector postResultAnimationDirector;

    [Header("Debug")]
    [SerializeField] private bool debugFinalImpact = true;

    private IBallState _currentState;

    public float SocketSmoothTime => socketSmoothTime;
    public float SocketMaxSpeed => socketMaxSpeed;
    public float SocketSnapDistance => socketSnapDistance;
    public float ShotIdleSpinSpeed => shotIdleSpinSpeed;

    public Transform GlovePoint => glovePoint;
    public Transform PlayerReference => playerReference;

    public float GoalContinueDistance => goalContinueDistance;
    public float GoalContinueDuration => goalContinueDuration;
    public float GoalContinueArcHeight => goalContinueArcHeight;

    public float GoalGroundY => goalGroundY;
    public float GoalDropDuration => goalDropDuration;
    public float GoalDropStartSpinMultiplier => goalDropStartSpinMultiplier;
    public float GoalDropForwardSlideDistance => goalDropForwardSlideDistance;

    public float OutContinueDistance => outContinueDistance;
    public float OutContinueDuration => outContinueDuration;
    public float OutContinueArcHeight => outContinueArcHeight;

    public float CatchSnapDuration => catchSnapDuration;
    public float CatchFollowSpinSpeed => catchFollowSpinSpeed;

    public float DeflectDistance => deflectDistance;
    public float DeflectDuration => deflectDuration;
    public float DeflectArcHeight => deflectArcHeight;
    public float DeflectSideOffset => deflectSideOffset;
    public float DeflectSpinSpeed => deflectSpinSpeed;

    public bool FinalImpactUseUnscaledTime => finalImpactUseUnscaledTime;
    public bool DebugFinalImpact => debugFinalImpact;

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

    public void EnterShotFlight(ShotBallFlightData flightData)
    {
        ChangeState(new BallShotFlightState(flightData));
    }

    public void EnterShotFinalImpact(ShotBallFlightData flightData)
    {
        ChangeState(new BallShotFinalImpactState(flightData));
    }

    public void SnapToSocketImmediate()
    {
        if (PlayerSocket == null)
            return;

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

    public void ReleaseBall()
    {
        ChangeState(null);
    }

    private void SetupPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }

    public void NotifyShotFinalImpact(ShotDecisionResult decision)
    {
        if (postResultAnimationDirector != null)
            postResultAnimationDirector.PlayPostResult(decision);
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


    // =====================================================================================

    [Header("Rainbow Flick")]
    [Tooltip("Rainbow başladığında topun ilk smooth gideceği referans nokta. Karakter root altında empty olabilir.")]
    [SerializeField] private Transform rainbowPrepPoint;

    [Tooltip("Topun mevcut pozisyondan RainbowPrepPoint'e kaç saniyede gideceği.")]
    [SerializeField] private float rainbowPrepDuration = 0.16f;

    [Tooltip("Prep noktasına giderken SmoothStep easing kullanılsın mı?")]
    [SerializeField] private bool rainbowUseSmoothPrep = true;

    [Tooltip("Prep noktasından sonraki yukarı/ileri uçuş süresi.")]
    [SerializeField] private float rainbowFlightDuration = 0.48f;

    [Tooltip("Top hareket sonunda oyuncunun önüne ne kadar düşsün?")]
    [SerializeField] private float rainbowForwardOffset = 2.6f;

    [Tooltip("Top en yüksek noktada zeminden ne kadar yukarı çıksın?")]
    [SerializeField] private float rainbowPeakHeight = 2.4f;

    [Tooltip("Topun hareket sonunda ineceği Y.")]
    [SerializeField] private float rainbowLandingY = -0.55f;

    [Tooltip("Rainbow sırasında topun öne dönüş hızı.")]
    [SerializeField] private float rainbowForwardSpinSpeed = 1400f;

    [Tooltip("Rainbow sırasında topun yana/curve dönüş hissi.")]
    [SerializeField] private float rainbowCurveSpinSpeed = 180f;

    [Tooltip("Top hareketi bittiğinde DribbleState'e geri dönsün mü?")]
    [SerializeField] private bool rainbowReturnToDribble = true;
    [Header("Rainbow Free Roll")]
    [SerializeField] private bool rainbowUseFreeRollAfterFlight = true;

    [Tooltip("Rainbow sonrası topun Z ekseninde geri yaklaşma hızı.")]
    [SerializeField] private float rainbowFreeRollZReturnSpeed = 2.5f;

    [Tooltip("Top tekrar dribble'a dönmek için oyuncunun X ekseninde ne kadar yakın olmalı?")]
    [SerializeField] private float rainbowFreeRollRecoverXDistance = 0.45f;

    [Tooltip("Top tekrar dribble'a dönmek için Z ekseninde oyuncuya ne kadar yakın olmalı?")]
    [SerializeField] private float rainbowFreeRollRecoverZDistance = 0.65f;

    [Tooltip("Free roll sırasında topun spin hızı.")]
    [SerializeField] private float rainbowFreeRollSpinSpeed = 650f;

    [Header("Rainbow Free Roll UI")]
    [SerializeField] private BallFreeRollIndicator freeRollIndicator;

    public BallFreeRollIndicator FreeRollIndicator => freeRollIndicator;

    // =====================================================================================


    [Header("Final Impact References")]
    [Tooltip("Clean catch durumlarında topun takip edeceği nokta. Keeper el/göğüs rig altında olabilir.")]
    [SerializeField] private Transform glovePoint;

    [Tooltip("Deflect/save rebound yönü için oyuncu veya kamera/player tarafı referansı.")]
    [SerializeField] private Transform playerReference;

    [Header("Final Impact - Goal Continue")]
    [SerializeField] private float goalContinueDistance = 7f;
    [SerializeField] private float goalContinueDuration = 0.35f;
    [SerializeField] private float goalContinueArcHeight = 0.6f;

    [Header("Final Impact - Goal Drop")]
    [Tooltip("Gol sonrası topun ineceği global zemin Y pozisyonu.")]
    [SerializeField] private float goalGroundY = -0.52f;

    [Tooltip("Top kalenin içine girdikten sonra zemine kaç saniyede insin?")]
    [SerializeField] private float goalDropDuration = 0.55f;

    [Tooltip("Yere inerken spin başlangıç çarpanı. 1 = mevcut spinSpeed, 0.5 = yarısı.")]
    [SerializeField] private float goalDropStartSpinMultiplier = 0.65f;

    [Tooltip("İniş sırasında çok hafif ekstra ileri kayma. Genelde 0 bırakabilirsin.")]
    [SerializeField] private float goalDropForwardSlideDistance = 0f;

    [Header("Final Impact - Out / Miss")]
    [SerializeField] private float outContinueDistance = 6f;
    [SerializeField] private float outContinueDuration = 0.4f;
    [SerializeField] private float outContinueArcHeight = 0.35f;

    [Header("Final Impact - Catch")]
    [SerializeField] private float catchSnapDuration = 0.08f;
    [SerializeField] private float catchFollowSpinSpeed = 450f;

    [Header("Final Impact - Deflect")]
    [SerializeField] private float deflectDistance = 8f;
    [SerializeField] private float deflectDuration = 0.45f;
    [SerializeField] private float deflectArcHeight = 1.2f;
    [SerializeField] private float deflectSideOffset = 1.5f;
    [SerializeField] private float deflectSpinSpeed = 1800f;

    [Header("Final Impact - Timing")]
    [SerializeField] private bool finalImpactUseUnscaledTime = true;

    [Header("Shot Result")]
    [SerializeField] private ShotPostResultAnimationDirector postResultAnimationDirector;

    [Header("Debug")]
    [SerializeField] private bool debugFinalImpact = true;

    private IBallState _currentState;

    public float SocketSmoothTime => socketSmoothTime;
    public float SocketMaxSpeed => socketMaxSpeed;
    public float SocketSnapDistance => socketSnapDistance;
    public float ShotIdleSpinSpeed => shotIdleSpinSpeed;

    public Transform RainbowPrepPoint => rainbowPrepPoint;
    public float RainbowPrepDuration => rainbowPrepDuration;
    public bool RainbowUseSmoothPrep => rainbowUseSmoothPrep;
    public float RainbowFlightDuration => rainbowFlightDuration;
    public float RainbowForwardOffset => rainbowForwardOffset;
    public float RainbowPeakHeight => rainbowPeakHeight;
    public float RainbowLandingY => rainbowLandingY;
    public float RainbowForwardSpinSpeed => rainbowForwardSpinSpeed;
    public float RainbowCurveSpinSpeed => rainbowCurveSpinSpeed;
    public bool RainbowReturnToDribble => rainbowReturnToDribble;
    public bool RainbowUseFreeRollAfterFlight => rainbowUseFreeRollAfterFlight;
    public float RainbowFreeRollZReturnSpeed => rainbowFreeRollZReturnSpeed;
    public float RainbowFreeRollRecoverXDistance => rainbowFreeRollRecoverXDistance;
    public float RainbowFreeRollRecoverZDistance => rainbowFreeRollRecoverZDistance;
    public float RainbowFreeRollSpinSpeed => rainbowFreeRollSpinSpeed;


    public Transform GlovePoint => glovePoint;
    public Transform PlayerReference => playerReference;

    public float GoalContinueDistance => goalContinueDistance;
    public float GoalContinueDuration => goalContinueDuration;
    public float GoalContinueArcHeight => goalContinueArcHeight;

    public float GoalGroundY => goalGroundY;
    public float GoalDropDuration => goalDropDuration;
    public float GoalDropStartSpinMultiplier => goalDropStartSpinMultiplier;
    public float GoalDropForwardSlideDistance => goalDropForwardSlideDistance;

    public float OutContinueDistance => outContinueDistance;
    public float OutContinueDuration => outContinueDuration;
    public float OutContinueArcHeight => outContinueArcHeight;

    public float CatchSnapDuration => catchSnapDuration;
    public float CatchFollowSpinSpeed => catchFollowSpinSpeed;

    public float DeflectDistance => deflectDistance;
    public float DeflectDuration => deflectDuration;
    public float DeflectArcHeight => deflectArcHeight;
    public float DeflectSideOffset => deflectSideOffset;
    public float DeflectSpinSpeed => deflectSpinSpeed;

    public bool FinalImpactUseUnscaledTime => finalImpactUseUnscaledTime;
    public bool DebugFinalImpact => debugFinalImpact;

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

    public void EnterRainbowFlick(Transform playerReference)
    {
        ChangeState(new BallRainbowFlickState(playerReference));
    }

    public void EnterShotFlight(ShotBallFlightData flightData)
    {
        ChangeState(new BallShotFlightState(flightData));
    }

    public void EnterShotFinalImpact(ShotBallFlightData flightData)
    {
        ChangeState(new BallShotFinalImpactState(flightData));
    }

    public void SnapToSocketImmediate()
    {
        if (PlayerSocket == null)
            return;

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

    public void ReleaseBall()
    {
        ChangeState(null);
    }

    private void SetupPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }

    public void NotifyShotFinalImpact(ShotDecisionResult decision)
    {
        if (postResultAnimationDirector != null)
            postResultAnimationDirector.PlayPostResult(decision);
    }

    public void EnterRainbowFreeRoll(Transform playerReference)
    {
        ChangeState(new BallRainbowFreeRollState(playerReference));
    }
}