using UnityEngine;

public class WorldCurveController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform _playerTransform;

    [Header("Curve Ayarları")]
    private float _maxCurveAmount = 0.0021f;
    [SerializeField] private float _maxPlayerX = 4f;

    // Shader Graph'taki referansın ID karşılığı
    private int _curvePropertyID;

    private void Start()
    {
        // GÖRSELDEN ALDIĞIMIZ REFERANS (Yazım hatası dahil birebir aynı olmalı)
        // Shader Graph Inspector'da yazan: _SideWayStrenght
        _curvePropertyID = Shader.PropertyToID("_SideWayStrenght");
    }

    private void LateUpdate()
    {
        if (_playerTransform == null) return;

        // MATEMATİK:
        // Karakter sağa (+X) giderse, yol sola (-Curve) bükülmeli.
        float ratio = -_playerTransform.position.x / _maxPlayerX;
        float currentCurve = ratio * _maxCurveAmount;

        // Debug.Log($"Curve Değeri: {currentCurve}"); // Çalışmıyorsa bu satırı açıp konsola bakabilirsin.

        // GLOBAL DEĞİŞKENİ GÜNCELLE:
        Shader.SetGlobalFloat(_curvePropertyID, currentCurve);
    }
}