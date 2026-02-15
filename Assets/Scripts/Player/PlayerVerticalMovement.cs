using UnityEngine;

public class PlayerVerticalMovement : MonoBehaviour, IJumper
{
    [Header("Referanslar")]
    public BallController Ball;

    [Header("Zıplama Ayarları")]
     private float jumpHeight = 4f; // Biraz artırdım daha net görülsün
     private float timeToJumpApex = 0.2f; 
     private float quickDropFactor = 4f; // Hızlı iniş daha keskin olsun

    public float JumpHeight => jumpHeight;
    public float TimeToJumpApex => timeToJumpApex;

    
    private float _gravity;
    private float _jumpVelocity;
    private float _verticalVelocity;
    private bool _isGrounded;
    private bool _isQuickDropping;

    private const float GroundLevel = 0f; // Zemin Yüksekliği

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

    public void Jump()
    {
        // Sadece yerdeyken zıpla
        if (_isGrounded)
        {
            _verticalVelocity = _jumpVelocity; // Hızı yukarı veriyoruz
            // Not: _isGrounded burada false yapmıyoruz, bir sonraki frame HandleGravity halledecek.

            if (Ball != null) Ball.OnPlayerJump();
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
        }
    }
}