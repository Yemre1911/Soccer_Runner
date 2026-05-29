/*using UnityEngine;
using System;

public class PlayerLateralMovement : MonoBehaviour, IMover
{
    [Header("Adım Ayarları")]
    [SerializeField] private float stepSize = 5f;
    [SerializeField] private float roadWidth = 10f;

    [Header("Hız Ayarları")]
    public float MovementSmoothTime = 0.2f;
    private float _maxSpeed = 30f;

    [Header("Görsel Efektler")]
    [Tooltip("Sağa/Sola giderken maksimum dönüş açısı.")]
    [SerializeField] private float turnAngle = 90f;

    public float TargetX { get; private set; }
    private float _currentVelocity;

    public event Action<int> OnLaneChangeRequested;

    private bool _movementLocked;
    private bool _autoRotationEnabled = true;

    private void Start()
    {
        TargetX = transform.position.x;
    }

    public void MoveStep(int direction)
    {
        if (_movementLocked)
            return;

        TargetX += direction * stepSize;
        TargetX = Mathf.Clamp(TargetX, -roadWidth, roadWidth);

        OnLaneChangeRequested?.Invoke(direction);
    }

    private void Update()
    {
        if (!_movementLocked)
        {
            float newX = Mathf.SmoothDamp(
                transform.position.x,
                TargetX,
                ref _currentVelocity,
                MovementSmoothTime,
                _maxSpeed);

            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        HandleRotation();
    }

    private void HandleRotation()
    {
        if (!_autoRotationEnabled)
            return;

        float velocityRatio = Mathf.Clamp(_currentVelocity / _maxSpeed, -1f, 1f);
        Quaternion targetRot = Quaternion.Euler(0f, velocityRatio * turnAngle, 0f);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }

    public void SetSpeed(float newSmoothTime)
    {
        MovementSmoothTime = Mathf.Clamp(newSmoothTime, 0.05f, 0.5f);
    }

    public void LockForShot()
    {
        _movementLocked = true;
        _autoRotationEnabled = false;

        TargetX = transform.position.x;
        _currentVelocity = 0f;
    }

    public void UnlockAfterShot()
    {
        _movementLocked = false;
        _autoRotationEnabled = true;

        TargetX = transform.position.x;
        _currentVelocity = 0f;
    }
}*/

using UnityEngine;
using System;

public class PlayerLateralMovement : MonoBehaviour, IMover
{
    [Header("Adım Ayarları")]
    [SerializeField] private float stepSize = 5f;
    [SerializeField] private float roadWidth = 10f;

    [Header("Hız Ayarları")]
    public float MovementSmoothTime = 0.2f;
    private float _maxSpeed = 30f;

    [Header("Görsel Efektler")]
    [Tooltip("Sağa/Sola giderken maksimum dönüş açısı.")]
    [SerializeField] private float turnAngle = 90f;

    public float TargetX { get; private set; }

    private float _currentVelocity;
    private bool _movementLocked;
    private bool _autoRotationEnabled = true;

    public event Action<int> OnLaneChangeRequested;

    private void Start()
    {
        TargetX = transform.position.x;
    }

    public void MoveStep(int direction)
    {
        if (_movementLocked)
            return;

        TargetX += direction * stepSize;
        TargetX = Mathf.Clamp(TargetX, -roadWidth, roadWidth);

        OnLaneChangeRequested?.Invoke(direction);
    }

    private void Update()
    {
        if (!_movementLocked)
        {
            float newX = Mathf.SmoothDamp(
                transform.position.x,
                TargetX,
                ref _currentVelocity,
                MovementSmoothTime,
                _maxSpeed);

            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        HandleRotation();
    }

    private void HandleRotation()
    {
        if (!_autoRotationEnabled)
            return;

        float velocityRatio = Mathf.Clamp(_currentVelocity / _maxSpeed, -1f, 1f);
        Quaternion targetRot = Quaternion.Euler(0f, velocityRatio * turnAngle, 0f);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }

    public void SetSpeed(float newSmoothTime)
    {
        MovementSmoothTime = Mathf.Clamp(newSmoothTime, 0.05f, 0.5f);
    }

    public void LockMovement()
    {
        _movementLocked = true;
        _autoRotationEnabled = false;

        TargetX = transform.position.x;
        _currentVelocity = 0f;
    }

    public void UnlockMovement()
    {
        _movementLocked = false;
        _autoRotationEnabled = true;

        TargetX = transform.position.x;
        _currentVelocity = 0f;
    }

    public void LockForShot()
    {
        LockMovement();
    }

    public void UnlockAfterShot()
    {
        UnlockMovement();
    }
}