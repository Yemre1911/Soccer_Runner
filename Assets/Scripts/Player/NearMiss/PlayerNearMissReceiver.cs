using System;
using System.Collections.Generic;
using UnityEngine;

public enum NearMissKind { Single, Double }

public readonly struct NearMissEvent
{
    public readonly NearMissKind Kind;
    public readonly int Direction;
    public NearMissEvent(NearMissKind kind, int direction)
    {
        Kind = kind;
        Direction = direction;
    }
}

[DisallowMultipleComponent]
public class PlayerNearMissReceiver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LaneChangeWindow window;
    [SerializeField] private PlayerCombatState combat;

    [Header("Immediate detection")]
    [Tooltip("NearMiss layer'ını seç (Opponent near-miss collider'larının layer'ı).")]
    [SerializeField] private LayerMask nearMissLayer;

    [Tooltip("Bu objenin CapsuleCollider'ı ile OverlapCapsule taraması yapar.")]
    [SerializeField] private CapsuleCollider receiverCapsule;

    public event Action<NearMissEvent> OnNearMiss;

    private readonly HashSet<int> _touchedThisWindow = new();

    private bool _invalidated;
    private bool _firedSingle;
    private bool _firedDouble;

    private void Awake()
    {
        if (window == null) window = GetComponentInParent<LaneChangeWindow>();
        if (combat == null) combat = GetComponentInParent<PlayerCombatState>();
        if (receiverCapsule == null) receiverCapsule = GetComponent<CapsuleCollider>();
    }

    private void OnEnable()
    {
        if (window != null)
        {
            window.OnWindowOpened += OnWindowOpened;
            window.OnWindowClosed += OnWindowClosed;
        }

        if (combat != null)
        {
            combat.OnStagger += Invalidate;
            combat.OnFall += Invalidate;
        }
    }

    private void OnDisable()
    {
        if (window != null)
        {
            window.OnWindowOpened -= OnWindowOpened;
            window.OnWindowClosed -= OnWindowClosed;
        }

        if (combat != null)
        {
            combat.OnStagger -= Invalidate;
            combat.OnFall -= Invalidate;
        }
    }

    private void OnWindowOpened()
    {
        _invalidated = false;
        _firedSingle = false;
        _firedDouble = false;
        _touchedThisWindow.Clear();

        // Window açıldığı anda mevcut overlap'ları yakala (hızlı swipe edge-case fix)
        RegisterCurrentOverlapsAndMaybeFireUI();
    }

    private void Invalidate()
    {
        if (window != null && window.IsOpen)
            _invalidated = true;
    }

    private void OnTriggerEnter(Collider other) => TryRegisterAndMaybeFire(other);
    private void OnTriggerStay(Collider other) => TryRegisterAndMaybeFire(other);

    private void TryRegisterAndMaybeFire(Collider other)
    {
        if (window == null || !window.IsOpen) return;
        if (_invalidated) return;

        if (other.GetComponent<OpponentNearMissZone>() == null) return;

        var opp = other.GetComponentInParent<OpponentIdentity>();
        if (opp == null) return;

        int id = opp.gameObject.GetInstanceID();

        // Yeni bir rakip mi?
        if (!_touchedThisWindow.Add(id)) return;

        // ✅ UI'yi ANINDA tetikle:
        int count = _touchedThisWindow.Count;

        if (!_firedSingle && !_firedDouble && count >= 1)
        {
            _firedSingle = true;
            OnNearMiss?.Invoke(new NearMissEvent(NearMissKind.Single, window.Direction));
            return;
        }

        if (!_firedDouble && count >= 2)
        {
            _firedDouble = true;
            OnNearMiss?.Invoke(new NearMissEvent(NearMissKind.Double, window.Direction));
        }
    }

    private void RegisterCurrentOverlapsAndMaybeFireUI()
    {
        if (receiverCapsule == null) return;
        if (nearMissLayer.value == 0) return;

        GetWorldCapsule(receiverCapsule, out Vector3 p1, out Vector3 p2, out float radius);

        Collider[] hits = Physics.OverlapCapsule(
            p1, p2, radius,
            nearMissLayer,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hits.Length; i++)
            TryRegisterAndMaybeFire(hits[i]);
    }

    private static void GetWorldCapsule(CapsuleCollider cap, out Vector3 p1, out Vector3 p2, out float radius)
    {
        Transform t = cap.transform;

        Vector3 scale = t.lossyScale;
        float maxXZ = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        radius = cap.radius * maxXZ;

        float height = Mathf.Max(cap.height * Mathf.Abs(scale.y), radius * 2f);
        Vector3 center = t.TransformPoint(cap.center);

        float offset = (height / 2f) - radius;

        Vector3 dir;
        if (cap.direction == 0) dir = t.right;
        else if (cap.direction == 1) dir = t.up;
        else dir = t.forward;

        p1 = center + dir * offset;
        p2 = center - dir * offset;
    }

    private void OnWindowClosed()
    {
        // UI artık anında tetiklendiği için burada ekstra bir şey yapmıyoruz.
        // İstersen ileride skor/combo finalize logic'i buraya koyarsın.
    }
}