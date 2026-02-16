using UnityEngine;

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
}
