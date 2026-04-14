/*using UnityEngine;
public class WorldScrollMover : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField] private Vector3 worldDirection = Vector3.back;

    [Header("Despawn Rule")]
    [Tooltip("Player'ın bu kadar arkasına düşerse despawn edilir.")]
    [SerializeField] private float despawnBehindPlayerZ = 15f;

    [Header("Optional")]
    [Tooltip("Player tag. Player objen bu tag'e sahip olmalı.")]
    [SerializeField] private string playerTag = "Player";

    private GameManager _gameManager;
    private Transform _player;
    private PooledItem _pooledItem;

    private void Awake()
    {
        _pooledItem = GetComponent<PooledItem>();

        if (worldDirection.sqrMagnitude < 0.0001f)
            worldDirection = Vector3.back;

        worldDirection.Normalize();
    }

    private void OnEnable()
    {
        ResolveRefs(force: true);
    }

    private void ResolveRefs(bool force = false)
    {
        if (force || _gameManager == null)
            _gameManager = GameManager.Instance != null
                ? GameManager.Instance
                : FindFirstObjectByType<GameManager>();

        if (force || _player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            _player = p != null ? p.transform : null;
        }
    }

    private void Update()
    {
        if (_gameManager == null || _player == null)
        {
            ResolveRefs();
            if (_gameManager == null) return;
        }

        float speed = _gameManager.CurrentSpeed;
        if (speed <= 0f) return;

        transform.position += worldDirection * speed * Time.deltaTime;

        if (_player != null && transform.position.z < (_player.position.z - despawnBehindPlayerZ))
            Despawn();
    }

    private void Despawn()
    {
        if (_pooledItem != null) _pooledItem.Despawn();
        else gameObject.SetActive(false);
    }
}*/


using UnityEngine;

public class WorldScrollMover : MonoBehaviour
{
    [Header("Direction")]
    [SerializeField] private Vector3 worldDirection = Vector3.back;

    [Header("Despawn Rule")]
    [Tooltip("Player'ın bu kadar arkasına düşerse despawn edilir.")]
    [SerializeField] private float despawnBehindPlayerZ = 15f;

    [Header("Optional")]
    [Tooltip("Player tag. Player objen bu tag'e sahip olmalı.")]
    [SerializeField] private string playerTag = "Player";

    private GameManager _gameManager;
    private Transform _player;
    private PooledItem _pooledItem;

    private void Awake()
    {
        _pooledItem = GetComponent<PooledItem>();

        if (worldDirection.sqrMagnitude < 0.0001f)
            worldDirection = Vector3.back;

        worldDirection.Normalize();
    }

    private void OnEnable()
    {
        ResolveRefs(force: true);
    }

    private void ResolveRefs(bool force = false)
    {
        if (force || _gameManager == null)
            _gameManager = GameManager.Instance != null
                ? GameManager.Instance
                : FindFirstObjectByType<GameManager>();

        if (force || _player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            _player = p != null ? p.transform : null;
        }
    }

    private void Update()
    {
        if (_gameManager == null || _player == null)
        {
            ResolveRefs();
            if (_gameManager == null) return;
        }

        if (_gameManager.SimulationPaused)
            return;

        float speed = _gameManager.CurrentSpeed;
        if (speed <= 0f) return;

        transform.position += worldDirection * speed * Time.deltaTime;

        if (_player != null && transform.position.z < (_player.position.z - despawnBehindPlayerZ))
            Despawn();
    }

    private void Despawn()
    {
        if (_pooledItem != null) _pooledItem.Despawn();
        else gameObject.SetActive(false);
    }
}