/*using UnityEngine;

public class GameSceneModeSetup : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private SessionLoader sessionLoader;
    [SerializeField] private SessionDirector sessionDirector;

    [Header("Disable In Shot Training")]
    [SerializeField] private GameObject[] disableForShotTraining;

    [Header("Road Setup")]
    [SerializeField] private GameObject trainingRoadRoot;
    [SerializeField] private Transform trainingRoadSpawnPoint;

    [SerializeField] private GameObject endRoadRoot;
    [SerializeField] private Transform endRoadSpawnPoint;

    [Header("Shot Flow")]
    [SerializeField] private ShotSequenceController shotSequenceController;

    private SessionConfig _config;

    private void Awake()
    {
        if (!sessionLoader) sessionLoader = FindFirstObjectByType<SessionLoader>();
        if (!sessionDirector) sessionDirector = FindFirstObjectByType<SessionDirector>();
    }

    private void Start()
    {
        _config = SessionBootstrap.PendingConfig ?? SessionBootstrap.LastLoadedConfig ?? SessionConfig.DefaultTest();

        if (_config.mode == SessionMode.ShotTraining)
            SetupShotTrainingMode();
        else
            SetupNormalRunMode();
    }

    private void SetupNormalRunMode()
    {
        if (trainingRoadRoot) trainingRoadRoot.SetActive(false);
        if (endRoadRoot) endRoadRoot.SetActive(false);
    }

    private void SetupShotTrainingMode()
    {
        // Gereksiz sistemleri kapat
        if (disableForShotTraining != null)
        {
            for (int i = 0; i < disableForShotTraining.Length; i++)
            {
                if (disableForShotTraining[i] != null)
                    disableForShotTraining[i].SetActive(false);
            }
        }

        // Training road aktif et ve konumlandır
        if (trainingRoadRoot)
        {
            trainingRoadRoot.SetActive(true);

            if (trainingRoadSpawnPoint != null)
            {
                trainingRoadRoot.transform.SetPositionAndRotation(
                    trainingRoadSpawnPoint.position,
                    trainingRoadSpawnPoint.rotation);
            }
        }

        // End road aktif et ve training road’un hemen sonuna yerleştir
        if (endRoadRoot)
        {
            endRoadRoot.SetActive(true);

            if (endRoadSpawnPoint != null)
            {
                endRoadRoot.transform.SetPositionAndRotation(
                    endRoadSpawnPoint.position,
                    endRoadSpawnPoint.rotation);
            }
        }

        if (shotSequenceController != null)
            shotSequenceController.EnterShotMode();
    }
}*/



using UnityEngine;

public class GameSceneModeSetup : MonoBehaviour
{
    [SerializeField] private SessionLoader sessionLoader;

    [Header("Disable In Shot Training")]
    [SerializeField] private GameObject[] disableForShotTraining;

    [Header("Training Layout")]
    [SerializeField] private GameObject trainingRoadRoot;
    [SerializeField] private GameObject endRoadRoot;

    [Header("Shot Flow")]
    [SerializeField] private ShotSequenceController shotSequenceController;

    private void Awake()
    {
        if (!sessionLoader)
            sessionLoader = FindFirstObjectByType<SessionLoader>();
    }

    /*private void Start()
    {
        var config = sessionLoader != null ? sessionLoader.ActiveConfig : null;
        if (config == null) return;

        if (config.mode == SessionMode.ShotTraining)
            SetupShotTrainingMode();
        else
            SetupNormalMode();
    }*/

    private void Start()
    {
        if (sessionLoader == null)
            return;

        var config = sessionLoader.ActiveConfig;

        if (config == null)
        {
            Debug.LogWarning("[GameSceneModeSetup] ActiveConfig is null in Start.");
            return;
        }

        if (config.mode == SessionMode.ShotTraining)
            SetupShotTrainingMode();
        else
            SetupNormalMode();
    }

    private void SetupNormalMode()
    {
        if (trainingRoadRoot) trainingRoadRoot.SetActive(false);
        if (endRoadRoot) endRoadRoot.SetActive(false);
    }

    private void SetupShotTrainingMode()
    {
        if (disableForShotTraining != null)
        {
            for (int i = 0; i < disableForShotTraining.Length; i++)
            {
                if (disableForShotTraining[i] != null)
                    disableForShotTraining[i].SetActive(false);
            }
        }

        if (trainingRoadRoot) trainingRoadRoot.SetActive(true);
        if (endRoadRoot) endRoadRoot.SetActive(true);

        /*var socket = trainingRoadRoot != null ? trainingRoadRoot.GetComponent<RoadSocket>() : null;
        if (socket != null && socket.EndAttachPoint != null && endRoadRoot != null)
        {
            endRoadRoot.transform.SetPositionAndRotation(
                socket.EndAttachPoint.position,
                socket.EndAttachPoint.rotation
            );
        }*/

        if (shotSequenceController != null)
            shotSequenceController.EnterShotMode();
    }
}