using UnityEngine;

public class JumpVisualTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject shoesVisualRoot;

    [Header("Boing")]
    [SerializeField] private bool enableBoing = true;
    [SerializeField] private Transform boingRoot;
    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float frequency = 8f;

    [Header("Optional animator slow")]
    [SerializeField] private Animator animator;
    [SerializeField] private float boostedAnimatorSpeedMultiplier = 0.9f;

    private Vector3 _initialLocalPos;
    private float _baseAnimatorSpeed = 1f;
    private bool _active;

    private void Awake()
    {
        if (!boingRoot) boingRoot = transform;

        _initialLocalPos = boingRoot.localPosition;

        if (animator) _baseAnimatorSpeed = animator.speed;

        SetActiveVisual(false);
    }

    private void Update()
    {
        if (!_active || !enableBoing) return;

        Vector3 pos = _initialLocalPos;
        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        boingRoot.localPosition = pos;
    }

    public void SetActiveVisual(bool active)
    {
        _active = active;

        if (shoesVisualRoot)
            shoesVisualRoot.SetActive(active);

        if (!active && boingRoot)
            boingRoot.localPosition = _initialLocalPos;
    }

    public void SetAnimatorBoostState(bool boosted)
    {
        if (!animator) return;

        animator.speed = boosted
            ? _baseAnimatorSpeed * boostedAnimatorSpeedMultiplier
            : _baseAnimatorSpeed;
    }
}