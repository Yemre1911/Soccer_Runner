using System.Collections;
using UnityEngine;

public class GameSceneModeSetup : MonoBehaviour
{
    [SerializeField] private SessionLoader sessionLoader;

    [Header("Disable In Shot Training")]
    [SerializeField] private GameObject[] disableForShotTraining;

    [Header("Training Layout")]
    [SerializeField] private GameObject trainingRoadRoot;
    [SerializeField] private GameObject endRoadRoot;

    private bool _normalShotRigPrepared;

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

        SessionConfig config = sessionLoader.ActiveConfig;

        if (config.mode == SessionMode.ShotTraining)
            SetupShotTrainingMode();
        else
            SetupNormalMode();
    }

    private void SetupNormalMode()
    {
        /*
         * ÖNEMLİ:
         * Training Last_Block kapatılmadan önce içindeki çalışan şut altyapısını
         * normal oyunun Last_Block objesine taşıyoruz. Böylece mevcut serialized
         * referansların tamamı (ShotKeeperController, hedef collider'ları,
         * MissedPoints, net tetikleyicileri vb.) bozulmadan kalıyor.
         */
        PrepareNormalModeShotRig();

        if (trainingRoadRoot)
            trainingRoadRoot.SetActive(false);

        if (endRoadRoot)
            endRoadRoot.SetActive(false);
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

        if (trainingRoadRoot)
            trainingRoadRoot.SetActive(true);

        if (endRoadRoot)
            endRoadRoot.SetActive(true);
    }

    private void PrepareNormalModeShotRig()
    {
        if (_normalShotRigPrepared)
            return;

        if (!endRoadRoot)
        {
            LogWarning("Training Last_Block (endRoadRoot) referansı eksik. Normal mod şut rig'i hazırlanamadı.");
            return;
        }

        InfiniteRoad normalEndingRoad = FindNormalEndingRoad();
        if (!normalEndingRoad)
        {
            LogWarning("Normal oyundaki ending Last_Block bulunamadı. Normal mod şut rig'i hazırlanamadı.");
            return;
        }

        Transform normalEndingRoot = normalEndingRoad.transform;
        Transform trainingEndingRoot = endRoadRoot.transform;

        Transform trainingGoal = FindDeepChild(trainingEndingRoot, "Kale");
        Transform normalGoal = FindDeepChild(normalEndingRoot, "Kale");

        if (!trainingGoal || !normalGoal)
        {
            LogWarning(
                $"Kale referansı çözülemedi. TrainingGoal={(trainingGoal ? trainingGoal.name : "NULL")}, " +
                $"NormalGoal={(normalGoal ? normalGoal.name : "NULL")}");
            return;
        }

        Transform trainingKeeper = trainingEndingRoot.Find("GoalKeeper");
        if (!trainingKeeper)
        {
            LogWarning("Training GoalKeeper bulunamadı.");
            return;
        }

        // Normal final bloğunda koşucu rakip prefab'ı kaleci gibi kullanılmış.
        // Çalışan keeper rig'iyle çakışmaması için onu kapatıp transformunu referans alıyoruz.
        Transform normalKeeperPlaceholder = FindNormalKeeperPlaceholder(normalEndingRoot);
        MoveKeeperToNormalEnding(trainingKeeper, normalEndingRoot, normalGoal, normalKeeperPlaceholder);

        MoveTrainingSupport(
            trainingEndingRoot.Find("ShootTargetSpace"),
            trainingGoal,
            normalGoal,
            normalEndingRoot,
            "ShootTargetSpace");

        MoveTrainingSupport(
            trainingEndingRoot.Find("GroundFailCollider"),
            trainingGoal,
            normalGoal,
            normalEndingRoot,
            "GroundFailCollider");

        MoveTrainingSupport(
            trainingEndingRoot.Find("MissedPoints"),
            trainingGoal,
            normalGoal,
            normalEndingRoot,
            "MissedPoints");

        // Normal kalede eksik olan görünmez ağ tetikleyicilerini ve çalışan net mesh'ini taşı.
        MoveTrainingSupport(
            trainingGoal.Find("NetTriggerObjects"),
            trainingGoal,
            normalGoal,
            normalGoal,
            "NetTriggerObjects");

        MoveTrainingSupport(
            trainingGoal.Find("Net_Final_MVP"),
            trainingGoal,
            normalGoal,
            normalGoal,
            "Net_Final_MVP");

        _normalShotRigPrepared = true;

        Log(
            $"Normal mode shot rig hazırlandı. " +
            $"TrainingRoot={trainingEndingRoot.name}, NormalRoot={normalEndingRoot.name}, " +
            $"NormalGoal={normalGoal.name}, Keeper={trainingKeeper.name}");
    }

    private InfiniteRoad FindNormalEndingRoad()
    {
        // İlk tercih: Shot Training'de kapatılan Stadium kökü içindeki recycle kapalı Last_Block.
        if (disableForShotTraining != null)
        {
            for (int i = 0; i < disableForShotTraining.Length; i++)
            {
                GameObject root = disableForShotTraining[i];
                if (!root)
                    continue;

                InfiniteRoad[] roads = root.GetComponentsInChildren<InfiniteRoad>(true);

                for (int j = 0; j < roads.Length; j++)
                {
                    InfiniteRoad road = roads[j];
                    if (IsNormalEndingRoadCandidate(road))
                        return road;
                }
            }
        }

        // Güvenli fallback: sahnedeki inactive objeler dahil bütün InfiniteRoad bileşenlerine bak.
        InfiniteRoad[] allRoads = Resources.FindObjectsOfTypeAll<InfiniteRoad>();

        for (int i = 0; i < allRoads.Length; i++)
        {
            InfiniteRoad road = allRoads[i];
            if (IsNormalEndingRoadCandidate(road))
                return road;
        }

        return null;
    }

    private bool IsNormalEndingRoadCandidate(InfiniteRoad road)
    {
        if (!road)
            return false;

        if (!road.gameObject.scene.IsValid())
            return false;

        if (endRoadRoot && road.gameObject == endRoadRoot)
            return false;

        if (!string.Equals(road.gameObject.name, "Last_Block", System.StringComparison.Ordinal))
            return false;

        // Ending parçası recycle edilmez. İsim çakışırsa bu ek kontrol doğru objeyi seçer.
        return !road.RecycleEnabled;
    }

    private Transform FindNormalKeeperPlaceholder(Transform normalEndingRoot)
    {
        ChasingOpponent[] opponents = normalEndingRoot.GetComponentsInChildren<ChasingOpponent>(true);

        for (int i = 0; i < opponents.Length; i++)
        {
            ChasingOpponent opponent = opponents[i];
            if (!opponent)
                continue;

            return opponent.transform;
        }

        return FindDeepChild(normalEndingRoot, "Op_Normal");
    }

    private void MoveKeeperToNormalEnding(
        Transform trainingKeeper,
        Transform normalEndingRoot,
        Transform normalGoal,
        Transform normalKeeperPlaceholder)
    {
        if (normalKeeperPlaceholder)
        {
            Vector3 placeholderLocalPosition = normalKeeperPlaceholder.localPosition;
            Quaternion placeholderLocalRotation = normalKeeperPlaceholder.localRotation;
            Vector3 placeholderLocalScale = normalKeeperPlaceholder.localScale;

            normalKeeperPlaceholder.gameObject.SetActive(false);

            trainingKeeper.SetParent(normalEndingRoot, false);
            trainingKeeper.localPosition = placeholderLocalPosition;
            trainingKeeper.localRotation = placeholderLocalRotation;
            trainingKeeper.localScale = placeholderLocalScale;
        }
        else
        {
            // Placeholder bulunamazsa training kalecisini kaleye göre oransal biçimde hizala.
            Transform trainingGoal = FindDeepChild(endRoadRoot.transform, "Kale");
            MoveRelativeToReference(trainingKeeper, trainingGoal, normalGoal, normalEndingRoot);
        }

        trainingKeeper.gameObject.SetActive(true);

        KeeperBasicMovement basicMovement = trainingKeeper.GetComponent<KeeperBasicMovement>();
        if (basicMovement)
            basicMovement.RebaseHomePosition();

        ShotKeeperController keeperController = trainingKeeper.GetComponent<ShotKeeperController>();
        if (keeperController)
            keeperController.ResetKeeperReaction();
    }

    private void MoveTrainingSupport(
        Transform item,
        Transform trainingGoal,
        Transform normalGoal,
        Transform newParent,
        string label)
    {
        if (!item)
        {
            LogWarning($"{label} bulunamadı; bu şut alt sistemi normal moda taşınamadı.");
            return;
        }

        MoveRelativeToReference(item, trainingGoal, normalGoal, newParent);
        item.gameObject.SetActive(true);

        Log($"{label} normal final bloğuna taşındı.");
    }

    private static void MoveRelativeToReference(
        Transform item,
        Transform sourceReference,
        Transform targetReference,
        Transform newParent)
    {
        if (!item || !sourceReference || !targetReference || !newParent)
            return;

        Vector3 relativePosition = sourceReference.InverseTransformPoint(item.position);
        Quaternion relativeRotation = Quaternion.Inverse(sourceReference.rotation) * item.rotation;
        Vector3 relativeScale = Divide(item.lossyScale, sourceReference.lossyScale);

        Vector3 targetWorldPosition = targetReference.TransformPoint(relativePosition);
        Quaternion targetWorldRotation = targetReference.rotation * relativeRotation;
        Vector3 targetWorldScale = Vector3.Scale(targetReference.lossyScale, relativeScale);

        item.SetParent(newParent, false);
        item.position = targetWorldPosition;
        item.rotation = targetWorldRotation;
        item.localScale = Divide(targetWorldScale, newParent.lossyScale);
    }

    private static Vector3 Divide(Vector3 value, Vector3 divisor)
    {
        return new Vector3(
            SafeDivide(value.x, divisor.x),
            SafeDivide(value.y, divisor.y),
            SafeDivide(value.z, divisor.z));
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) > 0.00001f ? value / divisor : value;
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (!root)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child && string.Equals(child.name, childName, System.StringComparison.Ordinal))
                return child;
        }

        return null;
    }

    private static void Log(string message)
    {
        Debug.Log("[GameSceneModeSetup] " + message);
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning("[GameSceneModeSetup] " + message);
    }
}
