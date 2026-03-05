using UnityEngine;

public class BreakEventBinder : MonoBehaviour
{
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private MonoBehaviour spawnEventBehaviour; // ISpawnEvent bekler
    private ISpawnEvent _spawnEvent;

    private void Awake()
    {
        if (sessionDirector == null)
            sessionDirector = FindFirstObjectByType<SessionDirector>();

        _spawnEvent = spawnEventBehaviour as ISpawnEvent;
    }

    private void OnEnable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged += Handle;
    }

    private void OnDisable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged -= Handle;
    }

    private void Handle(SessionDirector.Phase phase)
    {
        if (phase == SessionDirector.Phase.Break)
            _spawnEvent?.Trigger();
    }
}