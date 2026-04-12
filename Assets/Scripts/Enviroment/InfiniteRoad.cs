/*using UnityEngine;

public class InfiniteRoad : MonoBehaviour
{
    // Hızı artık buradan değil, GameManager'dan alacağız.
    // public float speed = 10f;  <-- Bunu siliyoruz.

    private float _planeLength;
    private int _totalPlanes = 4;

    void Start()
    {
        _planeLength = GetComponent<Renderer>().bounds.size.z;
    }

    void Update()
    {
        // Global hızı çek
        float currentSpeed = GameManager.Instance.CurrentSpeed;

        // O hıza göre hareket et
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime);

        // Başa sarma mantığın aynen kalıyor
        if (transform.position.z <= -_planeLength)   
        {
            Vector3 newPos = transform.position;
            newPos.z += _planeLength * _totalPlanes;
            transform.position = newPos;
        }
    }
}*/
using UnityEngine;

public class InfiniteRoad : MonoBehaviour
{
    [Header("Length")]
    [SerializeField] private Renderer lengthRenderer;
    [SerializeField] private float manualLength = 0f;

    [Header("Loop")]
    [SerializeField] private bool recycleEnabled = true;
    [SerializeField] private int totalPlanes = 4;

    private float _planeLength;

    public float PlaneLength => _planeLength;
    public bool RecycleEnabled => recycleEnabled;

    private void Awake()
    {
        CacheLength();
    }

    private void Start()
    {
        CacheLength();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        float currentSpeed = GameManager.Instance.CurrentSpeed;
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime, Space.World);

        if (recycleEnabled && transform.position.z <= -_planeLength)
        {
            Vector3 newPos = transform.position;
            newPos.z += _planeLength * totalPlanes;
            transform.position = newPos;
        }
    }

    public void SetRecycleEnabled(bool enabled)
    {
        recycleEnabled = enabled;
    }

    public void SetTotalPlanes(int count)
    {
        totalPlanes = Mathf.Max(1, count);
    }

    private void CacheLength()
    {
        if (manualLength > 0f)
        {
            _planeLength = manualLength;
            return;
        }

        if (lengthRenderer == null)
            lengthRenderer = GetComponentInChildren<Renderer>();

        if (lengthRenderer != null)
            _planeLength = lengthRenderer.bounds.size.z;

        if (_planeLength <= 0f)
            _planeLength = 1f;
    }
}