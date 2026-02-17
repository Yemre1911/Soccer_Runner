using UnityEngine;
public class PlayerCombatState : MonoBehaviour
{
    [SerializeField] private float staggerWindow = 1.5f;

    public bool IsFallen { get; private set; }
    public bool IsStaggered { get; private set; }

    private float _staggerTimer;

    public event System.Action OnStagger;
    public event System.Action OnFall;

    private void Update()
    {
        if (!IsStaggered) return;

        _staggerTimer -= Time.deltaTime;
        if (_staggerTimer <= 0f)
            IsStaggered = false;
    }

    public void OnHitByOpponent()
    {
        if (IsFallen) return;

        if (!IsStaggered)
        {
            IsStaggered = true;
            _staggerTimer = staggerWindow;
            OnStagger?.Invoke();
        }
        else
        {
            IsFallen = true;
            IsStaggered = false;
            OnFall?.Invoke();
        }
    }
}