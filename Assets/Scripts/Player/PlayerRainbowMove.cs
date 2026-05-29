using System.Collections;
using UnityEngine;

public class PlayerRainbowMove : MonoBehaviour, IRainbowMove
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerVerticalMovement verticalMovement;
    [SerializeField] private PlayerLateralMovement lateralMovement;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private HitResolver hitResolver;
    [SerializeField] private BallController ball;

    [Header("Rules")]
    [SerializeField] private float cooldown = 1.1f;

    [Tooltip("Animation Event unutulursa oyuncu sonsuza kadar kilitli kalmasın diye güvenlik süresi.")]
    [SerializeField] private float safetyUnlockDuration = 1.35f;

    [Tooltip("Rainbow sırasında player rakip/hazard hitlerinden etkilenmesin.")]
    [SerializeField] private bool ignorePlayerHitsDuringRainbow = true;

    [Tooltip("Rainbow sırasında sağ/sol hareket kilitlensin.")]
    [SerializeField] private bool lockLateralMovement = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private bool _isRainbowing;
    private float _nextAllowedTime;
    private Coroutine _safetyRoutine;

    private void Reset()
    {
        playerInput = GetComponent<PlayerInput>();
        verticalMovement = GetComponent<PlayerVerticalMovement>();
        lateralMovement = GetComponent<PlayerLateralMovement>();
        animationController = GetComponent<PlayerAnimationController>();
        hitResolver = GetComponent<HitResolver>();
        ball = GetComponentInChildren<BallController>();
    }

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (verticalMovement == null)
            verticalMovement = GetComponent<PlayerVerticalMovement>();

        if (lateralMovement == null)
            lateralMovement = GetComponent<PlayerLateralMovement>();

        if (animationController == null)
            animationController = GetComponent<PlayerAnimationController>();

        if (hitResolver == null)
            hitResolver = GetComponent<HitResolver>();
    }

    public bool TryRainbow()
    {
        if (!CanStartRainbow())
            return false;

        StartRainbow();
        return true;
    }

    private bool CanStartRainbow()
    {
        if (_isRainbowing)
            return false;

        if (Time.time < _nextAllowedTime)
            return false;

        if (verticalMovement == null || !verticalMovement.IsGrounded)
            return false;

        if (animationController == null)
            return false;

        if (GameOver.Instance != null && GameOver.Instance.IsGameOver)
            return false;

        return true;
    }

    private void StartRainbow()
    {
        _isRainbowing = true;
        _nextAllowedTime = Time.time + cooldown;

        if (debugLogs)
            Debug.Log("[PlayerRainbowMove] Rainbow started.", this);

        if (playerInput != null)
            playerInput.SetActionInputEnabled(false);

        if (lockLateralMovement && lateralMovement != null)
            lateralMovement.LockMovement();

        if (ignorePlayerHitsDuringRainbow && hitResolver != null)
            hitResolver.SetPlayerHitIgnored(true);

        animationController.PlayRainbow();

        if (_safetyRoutine != null)
            StopCoroutine(_safetyRoutine);

        _safetyRoutine = StartCoroutine(SafetyUnlockRoutine());
    }

    private IEnumerator SafetyUnlockRoutine()
    {
        yield return new WaitForSeconds(safetyUnlockDuration);

        if (_isRainbowing)
        {
            if (debugLogs)
                Debug.LogWarning("[PlayerRainbowMove] Safety unlock triggered. Did you forget OnRainbowFinished animation event?", this);

            FinishRainbow();
        }
    }

    // Animation Event: Topun ayaktan fırladığı frame'e koy.
    public void OnRainbowBallFlick()
    {
        if (!_isRainbowing)
            return;

        if (debugLogs)
            Debug.Log("[PlayerRainbowMove] Rainbow ball flick event.", this);

        if (ball != null)
            ball.EnterRainbowFlick(transform);
    }

    // Animation Event: Rainbow animasyonunun son frame'lerine koy.
    public void OnRainbowFinished()
    {
        if (!_isRainbowing)
            return;

        if (debugLogs)
            Debug.Log("[PlayerRainbowMove] Rainbow finished by animation event.", this);

        FinishRainbow();
    }

    private void FinishRainbow()
    {
        _isRainbowing = false;

        if (_safetyRoutine != null)
        {
            StopCoroutine(_safetyRoutine);
            _safetyRoutine = null;
        }

        if (ignorePlayerHitsDuringRainbow && hitResolver != null)
            hitResolver.SetPlayerHitIgnored(false);

        if (lockLateralMovement && lateralMovement != null)
            lateralMovement.UnlockMovement();

        if (playerInput != null)
            playerInput.SetActionInputEnabled(true);
    }

    private void OnDisable()
    {
        if (_isRainbowing)
            FinishRainbow();
    }
}