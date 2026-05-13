using UnityEngine;

public class KeeperBasicMovement : MonoBehaviour
{
    private enum PlayerXReadMode
    {
        WorldX,
        LocalX
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    [Header("Player X Read")]
    [SerializeField] private PlayerXReadMode playerXReadMode = PlayerXReadMode.LocalX;

    [Tooltip("Oyuncu ortadayken okunan X değeri. Genelde 0.")]
    [SerializeField] private float playerCenterX = 0f;

    [Tooltip("Oyuncunun merkezden sağ/sol maksimum uzaklığı. En sağ +10, en sol -10 ise bu değer 10 olmalı.")]
    [SerializeField] private float playerHalfRangeX = 10f;

    [Header("Keeper Movement")]
    [Tooltip("Kalecinin sağ/sol maksimum local X hareketi. +0.8 / -0.8 için 0.8 yaz.")]
    [SerializeField] private float keeperHalfRangeLocalX = 0.8f;

    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float walkThreshold = 0.001f;

    [Header("Animator Parameters")]
    [SerializeField] private string isWalkingParam = "IsWalking";
    [SerializeField] private string mirrorWalkParam = "MirrorWalk";

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Vector3 _startLocalPosition;
    private float _lastLocalX;
    private bool _isLocked;

    public bool IsLocked => _isLocked;

    private void Awake()
    {
        _startLocalPosition = transform.localPosition;
        _lastLocalX = transform.localPosition.x;

        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (animator)
            animator.applyRootMotion = false;
    }

    private void OnEnable()
    {
        PlayIdle();
    }

    private void Update()
    {
        if (_isLocked)
            return;

        if (!player)
        {
            PlayIdle();
            return;
        }

        float playerX = GetPlayerX();
        float targetKeeperX = GetTargetKeeperX(playerX);

        MoveKeeper(targetKeeperX);
        UpdateAnimation();

        if (debugLogs)
        {
            Debug.Log(
                $"[KeeperBasicMovement] PlayerX={playerX:F3} | " +
                $"Center={playerCenterX:F3} | HalfRange={playerHalfRangeX:F3} | " +
                $"Normalized={GetNormalizedPlayerX(playerX):F3} | " +
                $"TargetKeeperX={targetKeeperX:F3} | " +
                $"CurrentKeeperX={transform.localPosition.x:F3}"
            );
        }
    }

    public void SetLocked(bool locked)
    {
        _isLocked = locked;

        if (_isLocked)
        {
            PlayIdle();
        }
        else
        {
            _lastLocalX = transform.localPosition.x;
        }
    }

    private float GetPlayerX()
    {
        return playerXReadMode == PlayerXReadMode.WorldX
            ? player.position.x
            : player.localPosition.x;
    }

    private float GetNormalizedPlayerX(float playerX)
    {
        if (playerHalfRangeX <= 0.0001f)
            return 0f;

        float offsetFromCenter = playerX - playerCenterX;

        return Mathf.Clamp(
            offsetFromCenter / playerHalfRangeX,
            -1f,
            1f
        );
    }

    private float GetTargetKeeperX(float playerX)
    {
        float normalizedPlayerX = GetNormalizedPlayerX(playerX);
        return normalizedPlayerX * keeperHalfRangeLocalX;
    }

    private void MoveKeeper(float targetX)
    {
        Vector3 localPos = transform.localPosition;

        localPos.x = Mathf.MoveTowards(
            localPos.x,
            targetX,
            moveSpeed * Time.deltaTime
        );

        localPos.y = _startLocalPosition.y;
        localPos.z = _startLocalPosition.z;

        transform.localPosition = localPos;
    }

    private void UpdateAnimation()
    {
        float currentX = transform.localPosition.x;
        float deltaX = currentX - _lastLocalX;

        bool isWalking = Mathf.Abs(deltaX) > walkThreshold;

        if (animator)
        {
            animator.SetBool(isWalkingParam, isWalking);

            if (isWalking)
            {
                bool movingLeft = deltaX > 0f;
                animator.SetBool(mirrorWalkParam, movingLeft);
            }
        }

        _lastLocalX = currentX;
    }

    private void PlayIdle()
    {
        if (!animator)
            return;

        animator.SetBool(isWalkingParam, false);
    }
}