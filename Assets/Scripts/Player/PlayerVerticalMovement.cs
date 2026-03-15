/*using UnityEngine;
using System;
public class PlayerVerticalMovement : MonoBehaviour, IJumper
{
    [Header("Referanslar")]
    public BallController Ball;

    [Header("Zıplama Ayarları")]
    private float jumpHeight = 5f; // Biraz artırdım daha net görülsün
    private float timeToJumpApex = 0.3f;
    private float quickDropFactor = 4f; // Hızlı iniş daha keskin olsun

    public float JumpHeight => jumpHeight;
    public float TimeToJumpApex => timeToJumpApex;


    private float _gravity;
    private float _jumpVelocity;
    private float _verticalVelocity;
    private bool _isGrounded;
    private bool _isQuickDropping;

    private const float GroundLevel = 0f; // Zemin Yüksekliği


    // EVENTLER: Animasyon kontrolcüsü buralara abone olacak
    public event Action OnJumpPerformed;
    public event Action OnQuickDropPerformed;
    public event Action<bool> OnGroundedStatusChanged;

    private bool _lastGroundedStatus; // Durum değişimini takip etmek için

    private void Awake()
    {
        // Fizik formülleri
        _gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
    }

    private void Update()
    {
        HandleGravity();
        ApplyMovement();
        CheckGroundedChange(); 
    }

    private void HandleGravity()
    {
        // DÜZELTME BURADA:
        // Sadece yer seviyesindeysek VE aşağı düşüyorsak (veya duruyorsak) yer çekimini kapat.
        // Eğer zıpladıysak (_verticalVelocity > 0), bu bloğa girmeyecek ve yükselmeye devam edecek.
        if (transform.position.y <= GroundLevel && _verticalVelocity <= 0)
        {
            _verticalVelocity = -0.5f; // Yere sabitlemek için ufak kuvvet
            _isGrounded = true;
            _isQuickDropping = false;

            // Pozisyonu tam sıfıra çek (titremeyi önler)
            Vector3 fixPos = transform.position;
            fixPos.y = GroundLevel;
            transform.position = fixPos;

            return;
        }

        _isGrounded = false;

        // Yer çekimi uygulaması
        float currentGravity = _isQuickDropping ? _gravity * quickDropFactor : _gravity;
        _verticalVelocity += currentGravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        // Yer ve zıplama hareketi
        if (_isGrounded && _verticalVelocity < 0) return;

        Vector3 pos = transform.position;
        pos.y += _verticalVelocity * Time.deltaTime;
        transform.position = pos;
    }

    private void CheckGroundedChange()
    {
        // Gereksiz event fırlatmamak için sadece durum değiştiğinde haber ver
        if (_isGrounded != _lastGroundedStatus)
        {
            _lastGroundedStatus = _isGrounded;
            OnGroundedStatusChanged?.Invoke(_isGrounded);
        }
    }

    public void Jump()
    {
        // Sadece yerdeyken zıpla
        if (_isGrounded)
        {
            _verticalVelocity = _jumpVelocity; // Hızı yukarı veriyoruz
            if (Ball != null) Ball.OnPlayerJump();

            OnJumpPerformed?.Invoke();
        }
    }

    public void QuickDrop()
    {
        if (!_isGrounded)
        {
            _isQuickDropping = true;
            // Eğer hala yükseliyorsan, yükselmeyi iptal et ve düşüşe geç
            if (_verticalVelocity > 0) _verticalVelocity = 0;

            if (Ball != null) Ball.OnPlayerQuickDrop();

            OnQuickDropPerformed?.Invoke();
        }
    }
}*/

using UnityEngine;
using System;

public class PlayerVerticalMovement : MonoBehaviour, IJumper
{
    [Header("Referanslar")]
    public BallController Ball;

    [Header("Zıplama Ayarları")]
    [SerializeField] private float baseJumpHeight = 5f;
    [SerializeField] private float timeToJumpApex = 0.3f;
    [SerializeField] private float quickDropFactor = 4f;

    [Header("Runtime Boost")]
    [SerializeField] private float jumpHeightMultiplier = 1f;

    public float JumpHeight => baseJumpHeight * jumpHeightMultiplier;
    public float TimeToJumpApex => timeToJumpApex;
    public float JumpHeightMultiplier => jumpHeightMultiplier;

    private float _gravity;
    private float _jumpVelocity;
    private float _verticalVelocity;
    private bool _isGrounded;
    private bool _isQuickDropping;

    private const float GroundLevel = 0f;

    public event Action OnJumpPerformed;
    public event Action OnQuickDropPerformed;
    public event Action<bool> OnGroundedStatusChanged;

    private bool _lastGroundedStatus;

    private void Awake()
    {
        RecalculateJumpPhysics();
    }

    private void Update()
    {
        HandleGravity();
        ApplyMovement();
        CheckGroundedChange();
    }

    private void RecalculateJumpPhysics()
    {
        float effectiveJumpHeight = Mathf.Max(0.01f, baseJumpHeight * jumpHeightMultiplier);
        _gravity = -(2 * effectiveJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
    }

    private void HandleGravity()
    {
        if (transform.position.y <= GroundLevel && _verticalVelocity <= 0)
        {
            _verticalVelocity = -0.5f;
            _isGrounded = true;
            _isQuickDropping = false;

            Vector3 fixPos = transform.position;
            fixPos.y = GroundLevel;
            transform.position = fixPos;

            return;
        }

        _isGrounded = false;

        float currentGravity = _isQuickDropping ? _gravity * quickDropFactor : _gravity;
        _verticalVelocity += currentGravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        if (_isGrounded && _verticalVelocity < 0) return;

        Vector3 pos = transform.position;
        pos.y += _verticalVelocity * Time.deltaTime;
        transform.position = pos;
    }

    private void CheckGroundedChange()
    {
        if (_isGrounded != _lastGroundedStatus)
        {
            _lastGroundedStatus = _isGrounded;
            OnGroundedStatusChanged?.Invoke(_isGrounded);
        }
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            _verticalVelocity = _jumpVelocity;
            if (Ball != null) Ball.OnPlayerJump();

            OnJumpPerformed?.Invoke();
        }
    }

    public void QuickDrop()
    {
        if (!_isGrounded)
        {
            _isQuickDropping = true;

            if (_verticalVelocity > 0)
                _verticalVelocity = 0;

            if (Ball != null) Ball.OnPlayerQuickDrop();

            OnQuickDropPerformed?.Invoke();
        }
    }

    public void SetJumpHeightMultiplier(float multiplier)
    {
        jumpHeightMultiplier = Mathf.Max(0.1f, multiplier);
        RecalculateJumpPhysics();
    }

    public void ResetJumpHeightMultiplier()
    {
        jumpHeightMultiplier = 1f;
        RecalculateJumpPhysics();
    }
}