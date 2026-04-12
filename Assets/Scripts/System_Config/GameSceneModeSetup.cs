using UnityEngine;
using System.Collections;
public class GameSceneModeSetup : MonoBehaviour
{
    [SerializeField] private SessionLoader sessionLoader;

    [Header("Disable In Shot Training")]
    [SerializeField] private GameObject[] disableForShotTraining;

    [Header("Training Layout")]
    [SerializeField] private GameObject trainingRoadRoot;
    [SerializeField] private GameObject endRoadRoot;

    private void Awake()
    {
        if (!sessionLoader)
            sessionLoader = FindFirstObjectByType<SessionLoader>();
    }

    private IEnumerator Start()
    {
        if (!sessionLoader)
            sessionLoader = FindFirstObjectByType<SessionLoader>();

        yield return new WaitUntil(() => sessionLoader != null && sessionLoader.ActiveConfig != null);

        var config = sessionLoader.ActiveConfig;

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
    }
}