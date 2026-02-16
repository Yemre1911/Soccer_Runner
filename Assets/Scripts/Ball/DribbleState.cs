using UnityEngine;

public class DribbleState : IBallState
{
    private Vector3 _velocity;
    private PlayerLateralMovement _playerMovement;
    private ISprinter _playerSprinter;

    // --- AYARLAR ---
    private float _baseHeight = -0.55f;
    private float _arcHeight = 0.35f;

    // Normal vs Sprint Değerleri
    private float _normalFreq = 10f;
    private float _sprintFreq = 5f; // Sprintte ayaklar daha hızlı
    private float _normalAmp = 2f;
    private float _sprintAmp = 4f; // Sprintte top daha ileri açılır

    // Dinamik Değişkenler
    private float _currentAmp;
    private float _currentFreq;
    private float _sinePhase = 0f; // TİTREMEYİ ÖNLEYEN DEĞİŞKEN

    public void OnEnter(BallController ball)
    {
        if (ball.PlayerSocket != null)
        {
            _playerMovement = ball.PlayerSocket.GetComponentInParent<PlayerLateralMovement>();
            _playerSprinter = ball.PlayerSocket.GetComponentInParent<ISprinter>();
        }

        _currentAmp = _normalAmp;
        _currentFreq = _normalFreq;
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (ball.PlayerSocket == null || _playerMovement == null) return;

        bool isSprinting = (_playerSprinter != null) && _playerSprinter.IsSprinting;

        // 1. Değerleri Hedefe Yumuşakça Götür (Lerp)
        float targetAmp = isSprinting ? _sprintAmp : _normalAmp;
        float targetFreq = isSprinting ? _sprintFreq : _normalFreq;

        _currentAmp = Mathf.Lerp(_currentAmp, targetAmp, deltaTime * 3f);
        _currentFreq = Mathf.Lerp(_currentFreq, targetFreq, deltaTime * 3f);

        // 2. TİTREMESİZ SİNÜS HESABI
        // Time.time yerine kendi fazımızı biriktiriyoruz.
        _sinePhase += deltaTime * _currentFreq;
        // Sayı çok büyümesin diye mod alıyoruz (İsteğe bağlı ama sonsuz koşu için iyi)
        if (_sinePhase > Mathf.PI * 2) _sinePhase -= Mathf.PI * 2;

        float cycle = (Mathf.Sin(_sinePhase) + 1f) * 0.5f;


        // 3. POZİSYON HESAPLAMA
        float xDifference = Mathf.Abs(ball.transform.position.x - _playerMovement.TargetX);
        bool isChangingLane = xDifference > 0.1f;

        float targetZ;
        if (isChangingLane)
        {
            targetZ = ball.PlayerSocket.position.z;
            cycle = 0f; // Yana geçerken top yerde kalsın
        }
        else
        {
            targetZ = ball.PlayerSocket.position.z + (cycle * _currentAmp);
        }

        float targetY = _baseHeight + (cycle * _arcHeight);
        float targetX = _playerMovement.TargetX;

        // 4. HAREKETİ UYGULA
        Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);

        // Sprintte top biraz daha sert/hızlı tepki versin
        float smoothTime = isChangingLane ? 0.05f : (isSprinting ? 0.03f : 0.05f);
        float maxSpeed = isChangingLane ? 40f : (isSprinting ? 60f : 50f);

        ball.transform.position = Vector3.SmoothDamp(
            ball.transform.position, targetPosition, ref _velocity, smoothTime, maxSpeed, deltaTime
        );

        // 5. GÖRSEL DÖNME
        ball.transform.Rotate(Vector3.right * (isSprinting ? 1200f : 800f) * deltaTime, Space.World);
        ball.transform.Rotate(Vector3.forward * -_velocity.x * 3f * deltaTime, Space.World);
    }

    public void OnExit(BallController ball) { }
}