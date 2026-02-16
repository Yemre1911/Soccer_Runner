using UnityEngine;

public class PlayerSprint : MonoBehaviour, ISprinter
{
    [Header("Sprint Ayarları")]
    private float _sprintMultiplier = 1.85f;
    private float _acceleration = 0.22f; // Ne kadar sürede hızlansın? (Düşük = Yavaş, Yüksek = Atik)

    private float _targetMultiplier = 1f;
    private float _currentMultiplier = 1f;

    public bool IsSprinting { get; private set; }

    private void Update()
    {
        // Mevcut hızı hedefe doğru (araba ivmesi gibi) yaklaştır
        // Eğer hedef ile mevcut aynıysa işlem yapmaz (CPU dostu)
        if (!Mathf.Approximately(_currentMultiplier, _targetMultiplier))
        {
            _currentMultiplier = Mathf.MoveTowards(_currentMultiplier, _targetMultiplier, _acceleration * Time.deltaTime);
            GameManager.Instance.SetSpeedMultiplier(_currentMultiplier);
        }
    }

    public void StartSprint()
    {
        IsSprinting = true;
        _targetMultiplier = _sprintMultiplier; // Hedefi değiştir
    }

    public void StopSprint()
    {
        IsSprinting = false;
        _targetMultiplier = 1f; // Hedefi normale çek
    }
}