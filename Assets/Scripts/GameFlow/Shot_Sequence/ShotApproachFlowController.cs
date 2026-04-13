using System.Collections;
using UnityEngine;

public class ShotApproachFlowController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private SessionLoader sessionLoader;
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private ShotSequenceController shotSequenceController;
    [SerializeField] private FogDistanceController fogDistanceController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private BallController ballController;

    [Header("Goal Reference")]
    [SerializeField] private Transform goalTransform;
    [SerializeField] private Transform playerTransform;

    [Tooltip("Sahnede birden fazla kale/final block varsa hepsini buraya ver. Script aktif olanı otomatik seçer.")]
    [SerializeField] private Transform[] goalCandidates;

    [Header("Distances")]
    [SerializeField] private float shootingTriggerDistance = 18f;

    [Header("Fog")]
    [SerializeField] private float defaultFogDistance = 87f;
    [SerializeField] private float shotFogDistance = 117f;
    [SerializeField] private float normalEndingFogSmoothSpeed = 90f;

    [Header("Training Input Guard")]
    [SerializeField] private float trainingInputArmDelay = 0.35f;

    [Header("Goal Resolve")]
    [SerializeField] private bool logGoalResolve = true;
    [SerializeField] private float endingGoalResolveTimeout = 3f;

    private SessionConfig _config;
    private bool _trainingWaitingForInput;
    private bool _approachActive;
    private bool _shotStarted;
    private bool _initialized;
    private Coroutine _armInputRoutine;
    private Coroutine _waitForGoalRoutine;

    private void Awake()
    {
        if (!sessionLoader) sessionLoader = FindFirstObjectByType<SessionLoader>();
        if (!sessionDirector) sessionDirector = FindFirstObjectByType<SessionDirector>();
        if (!shotSequenceController) shotSequenceController = FindFirstObjectByType<ShotSequenceController>();
        if (!fogDistanceController) fogDistanceController = FindFirstObjectByType<FogDistanceController>();
        if (!playerInput) playerInput = FindFirstObjectByType<PlayerInput>();
        if (!ballController) ballController = FindFirstObjectByType<BallController>();

        if (!playerTransform)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        ResolveGoalTransform(false);
    }

    private void OnEnable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;

        if (playerInput != null)
            playerInput.OnAnyInputPressed += HandleAnyInputPressed;
    }

    private void OnDisable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;

        if (playerInput != null)
            playerInput.OnAnyInputPressed -= HandleAnyInputPressed;

        if (_armInputRoutine != null)
        {
            StopCoroutine(_armInputRoutine);
            _armInputRoutine = null;
        }

        if (_waitForGoalRoutine != null)
        {
            StopCoroutine(_waitForGoalRoutine);
            _waitForGoalRoutine = null;
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => sessionLoader != null && sessionLoader.ActiveConfig != null);

        _config = sessionLoader.ActiveConfig;
        InitializeForMode();
    }

    private void Update()
    {
        if (!_initialized || _config == null)
            return;

        if (_approachActive && !_shotStarted)
        {
            if (!IsGoalUsable(goalTransform))
                ResolveGoalTransform(false);

            CheckShootingDistance();
        }
    }

    private void InitializeForMode()
    {
        _initialized = true;
        _shotStarted = false;
        _approachActive = false;
        _trainingWaitingForInput = false;

        ResolveGoalTransform(true);

        if (_config.mode == SessionMode.ShotTraining)
            SetupTrainingMode();
        else
            SetupNormalMode();
    }

    private void SetupTrainingMode()
    {
        Debug.Log("[ShotApproach] SetupTrainingMode");

        if (fogDistanceController != null)
            fogDistanceController.SetImmediate(shotFogDistance);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetRunDistance();
            GameManager.Instance.SetSpeedMultiplier(0f);
            GameManager.Instance.SetSimulationPaused(true);
        }

        if (sessionDirector != null)
            sessionDirector.BeginManual(_config, SessionDirector.Phase.Idle);

        if (playerInput != null)
            playerInput.SetActionInputEnabled(false);

        if (ballController != null)
            ballController.EnterIdle();

        ResolveGoalTransform(true);

        _approachActive = false;
        _shotStarted = false;
        _trainingWaitingForInput = false;

        if (_armInputRoutine != null)
            StopCoroutine(_armInputRoutine);

        _armInputRoutine = StartCoroutine(ArmTrainingInput());
    }

    private void SetupNormalMode()
    {
        Debug.Log("[ShotApproach] SetupNormalMode");

        if (fogDistanceController != null)
            fogDistanceController.SetImmediate(defaultFogDistance);

        if (playerInput != null)
            playerInput.SetActionInputEnabled(true);

        if (ballController != null)
            ballController.EnterDribble();

        ResolveGoalTransform(true);

        _approachActive = false;
        _shotStarted = false;
        _trainingWaitingForInput = false;
    }

    private void HandleAnyInputPressed()
    {
        if (!_initialized || _config == null)
            return;

        if (_config.mode != SessionMode.ShotTraining)
            return;

        if (!_trainingWaitingForInput)
            return;

        Debug.Log("[ShotApproach] Training start input accepted.");
        StartTrainingRun();
    }

    private IEnumerator ArmTrainingInput()
    {
        Debug.Log("[ShotApproach] Arming training input...");

        float startRealtime = Time.realtimeSinceStartup;

        while (Input.GetMouseButton(0) || Input.touchCount > 0)
            yield return null;

        while (Time.realtimeSinceStartup - startRealtime < trainingInputArmDelay)
            yield return null;

        while (Input.GetMouseButton(0) || Input.touchCount > 0)
            yield return null;

        _trainingWaitingForInput = true;
        _armInputRoutine = null;

        Debug.Log("[ShotApproach] Training input armed.");
    }

    private void StartTrainingRun()
    {
        _trainingWaitingForInput = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetRunDistance();
            GameManager.Instance.SetSimulationPaused(false);
            GameManager.Instance.SetSpeedMultiplier(1f);
        }

        if (playerInput != null)
            playerInput.SetActionInputEnabled(true);

        if (ballController != null)
            ballController.EnterDribble();

        if (sessionDirector != null)
            sessionDirector.SetManualPhase(SessionDirector.Phase.Ending);

        ResolveGoalTransform(true);

        _approachActive = true;

        Debug.Log("[ShotApproach] Training run started.");
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        if (_config == null)
            return;

        Debug.Log("[ShotApproach] Phase changed -> " + phase);

        if (_config.mode != SessionMode.ShotTraining && phase == SessionDirector.Phase.Ending)
        {
            if (fogDistanceController != null)
                fogDistanceController.SmoothTo(shotFogDistance, normalEndingFogSmoothSpeed);

            _shotStarted = false;
            _approachActive = false;

            if (_waitForGoalRoutine != null)
                StopCoroutine(_waitForGoalRoutine);

            _waitForGoalRoutine = StartCoroutine(WaitForUsableGoalThenActivateApproach());

            Debug.Log("[ShotApproach] Normal mode ending detected. Waiting for active goal...");
        }
    }

    private IEnumerator WaitForUsableGoalThenActivateApproach()
    {
        float elapsed = 0f;

        while (elapsed < endingGoalResolveTimeout)
        {
            ResolveGoalTransform(false);

            if (IsGoalUsable(goalTransform))
            {
                _approachActive = true;
                _waitForGoalRoutine = null;

                Debug.Log("[ShotApproach] Active goal found. Shot approach active. Goal = " + goalTransform.name);
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _waitForGoalRoutine = null;

        Debug.LogWarning("[ShotApproach] Could not find an active goal in time. Approach not activated.");
    }

    private void CheckShootingDistance()
    {
        if (!IsGoalUsable(goalTransform))
            return;

        if (playerTransform == null)
        {
            Debug.LogWarning("[ShotApproach] playerTransform is NULL.");
            return;
        }

        float dz = Mathf.Abs(playerTransform.position.z - goalTransform.position.z);

        if (logGoalResolve)
        {
            Debug.Log(
                $"[ShotApproach] Checking distance | Player={playerTransform.position.z:F2} Goal={goalTransform.position.z:F2} dz={dz:F2}");
        }

        if (dz <= shootingTriggerDistance)
        {
            Debug.Log("[ShotApproach] Shooting distance reached. dz = " + dz);
            BeginShooting();
        }
    }

    private void BeginShooting()
    {
        if (_shotStarted)
            return;

        _shotStarted = true;
        _approachActive = false;

        Debug.Log("[ShotApproach] BeginShooting");

        if (GameManager.Instance != null)
            GameManager.Instance.SetSimulationPaused(true);

        if (shotSequenceController != null)
            shotSequenceController.EnterShotMode(playerTransform, goalTransform);
    }

    private bool ResolveGoalTransform(bool forceLog)
    {
        if (IsGoalUsable(goalTransform))
        {
            if (forceLog || logGoalResolve)
                Debug.Log("[ShotApproach] Goal already usable: " + goalTransform.name);

            return true;
        }

        if (goalCandidates != null)
        {
            for (int i = 0; i < goalCandidates.Length; i++)
            {
                Transform candidate = goalCandidates[i];
                if (!IsGoalUsable(candidate))
                    continue;

                goalTransform = candidate;

                if (forceLog || logGoalResolve)
                    Debug.Log("[ShotApproach] Goal resolved from candidates: " + goalTransform.name);

                return true;
            }
        }

        if (forceLog || logGoalResolve)
            Debug.LogWarning("[ShotApproach] No usable active goal found.");

        return false;
    }

    private bool IsGoalUsable(Transform target)
    {
        if (target == null)
            return false;

        if (!target.gameObject.activeInHierarchy)
            return false;

        return true;
    }

    public void SetGoalTransform(Transform newGoal)
    {
        goalTransform = newGoal;

        if (newGoal != null)
            Debug.Log("[ShotApproach] Goal transform assigned externally: " + newGoal.name);
        else
            Debug.LogWarning("[ShotApproach] Goal transform assigned externally as NULL.");
    }
}