using UnityEngine;

public class RoadEndingInjector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SessionDirector sessionDirector;

    [Tooltip("Sahnedeki normal, döngüsel yol parçaları.")]
    [SerializeField] private InfiniteRoad[] loopingRoadPieces;

    [Tooltip("Kale içeren özel yol parçası. Başta inactive olabilir.")]
    [SerializeField] private InfiniteRoad endingRoadPiece;

    [Header("Placement")]
    [Tooltip("Gerekirse z hizasına küçük offset ver.")]
    [SerializeField] private float extraZOffset = 0f;

    [Tooltip("Ending tile yerleştirilirken x/y current değerini korusun mu?")]
    [SerializeField] private bool preserveCurrentXY = true;

    private bool _injected;

    private void Awake()
    {
        if (sessionDirector == null)
            sessionDirector = FindFirstObjectByType<SessionDirector>();

        if ((loopingRoadPieces == null || loopingRoadPieces.Length == 0))
            loopingRoadPieces = FindObjectsByType<InfiniteRoad>(FindObjectsSortMode.None);

        // ending tile'ı normal listedeyse çıkaralım mantıksal olarak
        if (endingRoadPiece != null && loopingRoadPieces != null)
        {
            int count = 0;
            for (int i = 0; i < loopingRoadPieces.Length; i++)
            {
                if (loopingRoadPieces[i] != null && loopingRoadPieces[i] != endingRoadPiece)
                    count++;
            }

            InfiniteRoad[] filtered = new InfiniteRoad[count];
            int index = 0;

            for (int i = 0; i < loopingRoadPieces.Length; i++)
            {
                if (loopingRoadPieces[i] != null && loopingRoadPieces[i] != endingRoadPiece)
                    filtered[index++] = loopingRoadPieces[i];
            }

            loopingRoadPieces = filtered;
        }
    }

    private void OnEnable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        if (_injected) return;
        if (phase != SessionDirector.Phase.Ending) return;

        InjectEndingRoad();
    }

    private void InjectEndingRoad()
    {
        if (endingRoadPiece == null)
        {
            Debug.LogWarning("[RoadEndingInjector] Ending road piece is missing.");
            return;
        }

        if (loopingRoadPieces == null || loopingRoadPieces.Length == 0)
        {
            Debug.LogWarning("[RoadEndingInjector] No looping road pieces assigned.");
            return;
        }

        InfiniteRoad frontMost = null;
        float maxZ = float.MinValue;

        for (int i = 0; i < loopingRoadPieces.Length; i++)
        {
            InfiniteRoad road = loopingRoadPieces[i];
            if (road == null) continue;

            road.SetRecycleEnabled(false);

            if (road.transform.position.z > maxZ)
            {
                maxZ = road.transform.position.z;
                frontMost = road;
            }
        }

        if (frontMost == null)
        {
            Debug.LogWarning("[RoadEndingInjector] Could not find front-most road piece.");
            return;
        }

        float frontLength = frontMost.PlaneLength;
        float endingLength = endingRoadPiece.PlaneLength;

        GameObject endingGO = endingRoadPiece.gameObject;
        endingGO.SetActive(true);

        Vector3 pos = endingGO.transform.position;

        if (preserveCurrentXY)
        {
            pos.x = endingGO.transform.position.x;
            pos.y = endingGO.transform.position.y;
        }
        else
        {
            pos.x = frontMost.transform.position.x;
            pos.y = frontMost.transform.position.y;
        }

        pos.z = frontMost.transform.position.z + frontLength + extraZOffset;
        endingGO.transform.position = pos;

        // Bu parça da aksın ama recycle yapmasın
        endingRoadPiece.SetRecycleEnabled(false);

        _injected = true;

        Debug.Log("[RoadEndingInjector] Ending road injected successfully.");
    }
}