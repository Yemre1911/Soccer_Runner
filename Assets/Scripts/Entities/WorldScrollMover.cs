/*using UnityEngine;

/// <summary>
/// Prefab-safe world scrolling mover.
/// Scene referansı sürükleme gerekmez: GameManager/Player runtime bulunur.
/// </summary>
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

    private IGameSpeed _speedProvider;
    private Transform _player;
    private PooledItem _pooledItem;

    private void Awake()
    {
        _pooledItem = GetComponent<PooledItem>();

        // Direction normalize
        if (worldDirection.sqrMagnitude < 0.0001f) worldDirection = Vector3.back;
        worldDirection.Normalize();

        ResolveRefs();
    }

    private void OnEnable()
    {
        // Pool'dan çıkınca referanslar sahne değişmiş olabilir; yeniden çöz.
        ResolveRefs();
    }

    private void ResolveRefs()
    {
        // Speed provider (DIP): IGameSpeed arıyoruz.
        // 1) GameManager.Instance varsa onu kullan
        if (_speedProvider == null && GameManager.Instance != null)
            _speedProvider = GameManager.Instance;

        // 2) Bulamazsa sahneden IGameSpeed implement eden bir component ara (örn GameManager)
        if (_speedProvider == null)
        {
            var gm = FindFirstObjectByType<GameManager>();
            if (gm != null) _speedProvider = gm;
        }

        // Player
        if (_player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) _player = p.transform;
        }
    }

    private void Update()
    {
        if (_speedProvider == null) return;

        float speed = _speedProvider.CurrentSpeed;
        if (speed <= 0f) return;

        transform.position += worldDirection * speed * Time.deltaTime;

        // Player yoksa despawn kontrolü yapma (hareket yine de olur)
        if (_player != null)
        {
            if (transform.position.z < (_player.position.z - despawnBehindPlayerZ))
                Despawn();
        }
    }

    private void Despawn()
    {
        if (_pooledItem != null) _pooledItem.Despawn();
        else gameObject.SetActive(false);
    }
}*/

using UnityEngine;

/// <summary>
/// Prefab-safe world scrolling mover.
/// Scene referansı sürükleme gerekmez: GameManager/Player runtime bulunur.
/// </summary>
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
}