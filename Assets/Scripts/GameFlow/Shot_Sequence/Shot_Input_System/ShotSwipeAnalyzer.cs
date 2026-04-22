/*using UnityEngine;

public class ShotSwipeAnalyzer : MonoBehaviour
{
    [Header("Validation")]
    [SerializeField] private int minPointCount = 3;

    [Tooltip("Minimum toplam swipe uzunluğu = ekran diyagonalinin bu oranı kadar.")]
    [SerializeField] private float minSwipeScreenDiagonalPercent = 0.075f;

    [Tooltip("Ekran küçük olsa bile minimum toplam swipe uzunluğu bundan aşağı düşmez.")]
    [SerializeField] private float minSwipePixelsFloor = 70f;

    [Tooltip("Start-end düz mesafe = ekran diyagonalinin bu oranı kadar olmalı.")]
    [SerializeField] private float minDirectLengthScreenDiagonalPercent = 0.018f;

    [Tooltip("Ekran küçük olsa bile minimum düz mesafe bundan aşağı düşmez.")]
    [SerializeField] private float minDirectPixelsFloor = 18f;

    [Header("Curve Thresholds")]
    [SerializeField] private float straightCurveThreshold = 0.10f;
    [SerializeField] private float overCurveThreshold = 0.42f;



    [Header("ZigZag Detection")]
    [SerializeField] private int zigZagSignChangeLimit = 5;
    [SerializeField] private float zigZagMetricThreshold = 0.72f;
    [SerializeField] private float smoothnessHardFailThreshold = 0.12f;

    [Tooltip("Bu pikselden kısa segmentler zigzag hesabında yok sayılır.")]
    [SerializeField] private float minSegmentPixelsForTurnAnalysis = 18f;

    [Tooltip("Bu açıdan küçük kırılımlar turn olarak sayılmaz.")]
    [SerializeField] private float minAngleForTurnSign = 12f;

    [Tooltip("Swipe ana hattına göre bu kadar küçük yan sapmalar sign change üretmez.")]
    [SerializeField] private float lateralDeadZonePixels = 14f;

    [Tooltip("Analizde her N noktadan biri kullanılır. Jitter'ı azaltır.")]
    [SerializeField] private int pointStepForTurnAnalysis = 2;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    public ShotSwipeIntent Analyze(RawSwipeData raw)
    {
        ShotSwipeIntent intent = new ShotSwipeIntent();

        if (raw == null || raw.Points == null || raw.Points.Count == 0)
        {
            intent.IsValid = false;
            intent.Kind = ShotSwipeKind.Invalid;
            return intent;
        }

        intent.ScreenStart = raw.StartPoint;
        intent.ScreenEnd = raw.EndPoint;
        intent.EndedByTimeout = raw.EndedByTimeout;
        intent.PointCount = raw.Points.Count;

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        float screenDiagonal = Mathf.Sqrt((screenWidth * screenWidth) + (screenHeight * screenHeight));

        float minSwipeLengthNeeded = Mathf.Max(minSwipePixelsFloor, screenDiagonal * minSwipeScreenDiagonalPercent);
        float minDirectLengthNeeded = Mathf.Max(minDirectPixelsFloor, screenDiagonal * minDirectLengthScreenDiagonalPercent);

        intent.ScreenWidth = screenWidth;
        intent.ScreenHeight = screenHeight;
        intent.ScreenDiagonal = screenDiagonal;
        intent.MinSwipeLengthNeeded = minSwipeLengthNeeded;
        intent.MinDirectLengthNeeded = minDirectLengthNeeded;

        float totalLength = CalculateTotalLength(raw);
        float directLength = Vector2.Distance(raw.StartPoint, raw.EndPoint);

        intent.TotalLength = totalLength;
        intent.DirectLength = directLength;
        intent.FinalScreenDirection = CalculateFinalDirection(raw);

        bool tooFewPoints = raw.Points.Count < minPointCount;
        bool tooShort = totalLength < minSwipeLengthNeeded || directLength < minDirectLengthNeeded;

        intent.TooShort = tooShort;
        intent.TooLong = false; // artık çok uzun reddi yok

        if (tooFewPoints || tooShort)
        {
            intent.IsValid = false;
            intent.Kind = ShotSwipeKind.Invalid;
            intent.Power01 = Mathf.Clamp01(totalLength / Mathf.Max(1f, minSwipeLengthNeeded));
            Log(intent, "Rejected: too few points / too short");
            return intent;
        }

        float signedCurve = CalculateSignedCurve(raw);
        float curveAbs = Mathf.Abs(signedCurve);
        float smoothness = CalculateSmoothness(raw, out int signChanges, out float zigZagMetric);

        intent.CurveSigned01 = Mathf.Clamp(signedCurve, -1f, 1f);
        intent.CurveAbs01 = Mathf.Clamp01(curveAbs);
        intent.Smoothness01 = smoothness;
        intent.Quality01 = smoothness;
        intent.DirectionSignChanges = signChanges;
        intent.ZigZag01 = zigZagMetric;

        // Artık maksimuma göre ceza vermiyoruz.
        // Power01 sadece debug/relative okuma için kullanılıyor.
        intent.Power01 = Mathf.Clamp01(totalLength / Mathf.Max(1f, minSwipeLengthNeeded * 2f));
        intent.IsValid = true;

        bool isZigZag =
    signChanges >= zigZagSignChangeLimit ||
    (zigZagMetric >= zigZagMetricThreshold && curveAbs > 0.06f) ||
    smoothness <= smoothnessHardFailThreshold;

        if (isZigZag)
            intent.Kind = ShotSwipeKind.ZigZag;
        else if (curveAbs >= overCurveThreshold)
            intent.Kind = ShotSwipeKind.OverCurve;
        else if (curveAbs <= straightCurveThreshold)
            intent.Kind = ShotSwipeKind.Straight;
        else
            intent.Kind = ShotSwipeKind.Curve;

        Log(intent, "Analyze complete");
        return intent;
    }

    private float CalculateTotalLength(RawSwipeData raw)
    {
        float total = 0f;

        for (int i = 1; i < raw.Points.Count; i++)
            total += Vector2.Distance(raw.Points[i - 1], raw.Points[i]);

        return total;
    }

    private Vector2 CalculateFinalDirection(RawSwipeData raw)
    {
        if (raw.Points == null || raw.Points.Count < 2)
            return Vector2.up;

        int last = raw.Points.Count - 1;

        for (int i = last; i > 0; i--)
        {
            Vector2 delta = raw.Points[i] - raw.Points[i - 1];
            if (delta.sqrMagnitude > 0.0001f)
                return delta.normalized;
        }

        Vector2 fallback = raw.EndPoint - raw.StartPoint;
        return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector2.up;
    }

    private float CalculateSignedCurve(RawSwipeData raw)
    {
        Vector2 a = raw.StartPoint;
        Vector2 b = raw.EndPoint;
        Vector2 ab = b - a;
        float abLen = ab.magnitude;

        if (abLen < 0.001f)
            return 0f;

        Vector2 perp = new Vector2(-ab.y, ab.x).normalized;
        float signedDistanceSum = 0f;
        int sampleCount = 0;

        for (int i = 1; i < raw.Points.Count - 1; i++)
        {
            Vector2 ap = raw.Points[i] - a;
            signedDistanceSum += Vector2.Dot(ap, perp);
            sampleCount++;
        }

        if (sampleCount == 0)
            return 0f;

        float avgSignedDist = signedDistanceSum / sampleCount;
        return Mathf.Clamp(avgSignedDist / abLen, -1f, 1f);
    }

    private float CalculateSmoothness(RawSwipeData raw, out int signChanges, out float zigZagMetric)
    {
        signChanges = 0;
        zigZagMetric = 0f;

        if (raw == null || raw.Points == null || raw.Points.Count < 3)
            return 1f;

        Vector2 start = raw.StartPoint;
        Vector2 end = raw.EndPoint;
        Vector2 main = end - start;

        if (main.sqrMagnitude < 0.0001f)
            return 1f;

        Vector2 mainDir = main.normalized;
        Vector2 perp = new Vector2(-mainDir.y, mainDir.x);

        float angleSum = 0f;
        int angleCount = 0;

        int previousStableSide = 0;

        int step = Mathf.Max(1, pointStepForTurnAnalysis);

        for (int i = step; i < raw.Points.Count; i += step)
        {
            int prevIndex = Mathf.Max(0, i - step);

            Vector2 p0 = raw.Points[prevIndex];
            Vector2 p1 = raw.Points[i];

            Vector2 seg = p1 - p0;
            float segLen = seg.magnitude;

            if (segLen < minSegmentPixelsForTurnAnalysis)
                continue;

            Vector2 segDir = seg / segLen;
            float angle = Vector2.Angle(mainDir, segDir);

            if (angle >= minAngleForTurnSign)
            {
                angleSum += angle;
                angleCount++;
            }

            float sideDist = Vector2.Dot(p1 - start, perp);

            int stableSide = 0;
            if (sideDist > lateralDeadZonePixels) stableSide = 1;
            else if (sideDist < -lateralDeadZonePixels) stableSide = -1;

            if (stableSide != 0)
            {
                if (previousStableSide != 0 && stableSide != previousStableSide)
                    signChanges++;

                previousStableSide = stableSide;
            }
        }

        float avgAngle = angleCount > 0 ? angleSum / angleCount : 0f;

        float anglePenalty = Mathf.InverseLerp(18f, 95f, avgAngle);
        float signChangePenalty = Mathf.Clamp01(signChanges / Mathf.Max(1f, zigZagSignChangeLimit));

        zigZagMetric = Mathf.Clamp01((anglePenalty * 0.45f) + (signChangePenalty * 0.55f));
        return Mathf.Clamp01(1f - zigZagMetric);
    }

    private void Log(ShotSwipeIntent intent, string prefix)
    {
        if (!debugLogs)
            return;

        Debug.Log(
            $"[ShotSwipeAnalyzer] {prefix} | " +
            $"Screen={intent.ScreenWidth}x{intent.ScreenHeight} | Diagonal={intent.ScreenDiagonal:F1} | " +
            $"MinSwipeNeed={intent.MinSwipeLengthNeeded:F1} | MinDirectNeed={intent.MinDirectLengthNeeded:F1} | " +
            $"Valid={intent.IsValid} | Kind={intent.Kind} | Points={intent.PointCount} | " +
            $"Total={intent.TotalLength:F1} | Direct={intent.DirectLength:F1} | Power={intent.Power01:F2} | " +
            $"Curve={intent.CurveSigned01:F2} | CurveAbs={intent.CurveAbs01:F2} | " +
            $"Smooth={intent.Smoothness01:F2} | ZigZag={intent.ZigZag01:F2} | " +
            $"SignChanges={intent.DirectionSignChanges} | FinalDir={intent.FinalScreenDirection}");
    }
}*/

using UnityEngine;

public class ShotSwipeAnalyzer : MonoBehaviour
{
    [Header("Validation")]
    [SerializeField] private int minPointCount = 3;

    [Tooltip("Minimum toplam swipe uzunluğu = ekran diyagonalinin bu oranı kadar.")]
    [SerializeField] private float minSwipeScreenDiagonalPercent = 0.075f;

    [Tooltip("Ekran küçük olsa bile minimum toplam swipe uzunluğu bundan aşağı düşmez.")]
    [SerializeField] private float minSwipePixelsFloor = 70f;

    [Tooltip("Start-end düz mesafe = ekran diyagonalinin bu oranı kadar olmalı.")]
    [SerializeField] private float minDirectLengthScreenDiagonalPercent = 0.018f;

    [Tooltip("Ekran küçük olsa bile minimum düz mesafe bundan aşağı düşmez.")]
    [SerializeField] private float minDirectPixelsFloor = 18f;

    [Header("Curve Thresholds")]
    [SerializeField] private float straightCurveThreshold = 0.10f;
    [SerializeField] private float overCurveThreshold = 0.42f;

    [Header("ZigZag Detection")]
    [SerializeField] private int zigZagSignChangeLimit = 5;
    [SerializeField] private float zigZagMetricThreshold = 0.72f;
    [SerializeField] private float smoothnessHardFailThreshold = 0.12f;

    [Tooltip("Bu pikselden kısa segmentler zigzag hesabında yok sayılır.")]
    [SerializeField] private float minSegmentPixelsForTurnAnalysis = 18f;

    [Tooltip("Bu açıdan küçük kırılımlar turn olarak sayılmaz.")]
    [SerializeField] private float minAngleForTurnSign = 12f;

    [Tooltip("Swipe ana hattına göre bu kadar küçük yan sapmalar sign change üretmez.")]
    [SerializeField] private float lateralDeadZonePixels = 14f;

    [Tooltip("Analizde her N noktadan biri kullanılır. Jitter'ı azaltır.")]
    [SerializeField] private int pointStepForTurnAnalysis = 2;

    [Header("Malformed Path")]
    [SerializeField] private float minPathEfficiency = 0.58f;
    [SerializeField] private float malformedSmoothnessMax = 0.60f;
    [SerializeField] private float malformedCurveMin = 0.18f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    public ShotSwipeIntent Analyze(RawSwipeData raw)
    {
        ShotSwipeIntent intent = new ShotSwipeIntent();

        if (raw == null || raw.Points == null || raw.Points.Count == 0)
        {
            intent.IsValid = false;
            intent.Kind = ShotSwipeKind.Invalid;
            return intent;
        }

        intent.ScreenStart = raw.StartPoint;
        intent.ScreenEnd = raw.EndPoint;
        intent.EndedByTimeout = raw.EndedByTimeout;
        intent.PointCount = raw.Points.Count;

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        float screenDiagonal = Mathf.Sqrt((screenWidth * screenWidth) + (screenHeight * screenHeight));

        float minSwipeLengthNeeded = Mathf.Max(minSwipePixelsFloor, screenDiagonal * minSwipeScreenDiagonalPercent);
        float minDirectLengthNeeded = Mathf.Max(minDirectPixelsFloor, screenDiagonal * minDirectLengthScreenDiagonalPercent);

        intent.ScreenWidth = screenWidth;
        intent.ScreenHeight = screenHeight;
        intent.ScreenDiagonal = screenDiagonal;
        intent.MinSwipeLengthNeeded = minSwipeLengthNeeded;
        intent.MinDirectLengthNeeded = minDirectLengthNeeded;

        float totalLength = CalculateTotalLength(raw);
        float directLength = Vector2.Distance(raw.StartPoint, raw.EndPoint);

        intent.TotalLength = totalLength;
        intent.DirectLength = directLength;
        intent.FinalScreenDirection = CalculateFinalDirection(raw);

        bool tooFewPoints = raw.Points.Count < minPointCount;
        bool tooShort = totalLength < minSwipeLengthNeeded || directLength < minDirectLengthNeeded;

        intent.TooShort = tooShort;
        intent.TooLong = false;

        if (tooFewPoints || tooShort)
        {
            intent.IsValid = false;
            intent.Kind = ShotSwipeKind.Invalid;
            intent.Power01 = Mathf.Clamp01(totalLength / Mathf.Max(1f, minSwipeLengthNeeded));
            intent.PathEfficiency01 = directLength <= 0.001f ? 0f : Mathf.Clamp01(directLength / Mathf.Max(1f, totalLength));
            Log(intent, "Rejected: too few points / too short");
            return intent;
        }

        float signedCurve = CalculateSignedCurve(raw);
        float curveAbs = Mathf.Abs(signedCurve);

        float smoothness = CalculateSmoothness(raw, out int signChanges, out float zigZagMetric);

        float efficiency = directLength <= 0.001f
            ? 0f
            : Mathf.Clamp01(directLength / Mathf.Max(1f, totalLength));

        intent.CurveSigned01 = Mathf.Clamp(signedCurve, -1f, 1f);
        intent.CurveAbs01 = Mathf.Clamp01(curveAbs);
        intent.Smoothness01 = smoothness;
        intent.Quality01 = smoothness;
        intent.DirectionSignChanges = signChanges;
        intent.ZigZag01 = zigZagMetric;
        intent.PathEfficiency01 = efficiency;

        intent.Power01 = Mathf.Clamp01(totalLength / Mathf.Max(1f, minSwipeLengthNeeded * 2f));
        intent.IsValid = true;

        bool malformedPath =
            efficiency < minPathEfficiency &&
            smoothness <= malformedSmoothnessMax &&
            curveAbs >= malformedCurveMin;

        intent.MalformedPath = malformedPath;

        if (malformedPath)
        {
            intent.Kind = ShotSwipeKind.ZigZag;
            Log(intent, "Analyze complete (malformed path)");
            return intent;
        }

        bool isZigZag =
            signChanges >= zigZagSignChangeLimit ||
            (zigZagMetric >= zigZagMetricThreshold && curveAbs > 0.06f) ||
            smoothness <= smoothnessHardFailThreshold;

        if (isZigZag)
            intent.Kind = ShotSwipeKind.ZigZag;
        else if (curveAbs >= overCurveThreshold)
            intent.Kind = ShotSwipeKind.OverCurve;
        else if (curveAbs <= straightCurveThreshold)
            intent.Kind = ShotSwipeKind.Straight;
        else
            intent.Kind = ShotSwipeKind.Curve;

        Log(intent, "Analyze complete");
        return intent;
    }

    private float CalculateTotalLength(RawSwipeData raw)
    {
        float total = 0f;

        for (int i = 1; i < raw.Points.Count; i++)
            total += Vector2.Distance(raw.Points[i - 1], raw.Points[i]);

        return total;
    }

    private Vector2 CalculateFinalDirection(RawSwipeData raw)
    {
        if (raw.Points == null || raw.Points.Count < 2)
            return Vector2.up;

        int last = raw.Points.Count - 1;

        for (int i = last; i > 0; i--)
        {
            Vector2 delta = raw.Points[i] - raw.Points[i - 1];
            if (delta.sqrMagnitude > 0.0001f)
                return delta.normalized;
        }

        Vector2 fallback = raw.EndPoint - raw.StartPoint;
        return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector2.up;
    }

    private float CalculateSignedCurve(RawSwipeData raw)
    {
        Vector2 a = raw.StartPoint;
        Vector2 b = raw.EndPoint;
        Vector2 ab = b - a;
        float abLen = ab.magnitude;

        if (abLen < 0.001f)
            return 0f;

        Vector2 perp = new Vector2(-ab.y, ab.x).normalized;
        float signedDistanceSum = 0f;
        int sampleCount = 0;

        for (int i = 1; i < raw.Points.Count - 1; i++)
        {
            Vector2 ap = raw.Points[i] - a;
            signedDistanceSum += Vector2.Dot(ap, perp);
            sampleCount++;
        }

        if (sampleCount == 0)
            return 0f;

        float avgSignedDist = signedDistanceSum / sampleCount;
        return Mathf.Clamp(avgSignedDist / abLen, -1f, 1f);
    }

    private float CalculateSmoothness(RawSwipeData raw, out int signChanges, out float zigZagMetric)
    {
        signChanges = 0;
        zigZagMetric = 0f;

        if (raw == null || raw.Points == null || raw.Points.Count < 3)
            return 1f;

        Vector2 start = raw.StartPoint;
        Vector2 end = raw.EndPoint;
        Vector2 main = end - start;

        if (main.sqrMagnitude < 0.0001f)
            return 1f;

        Vector2 mainDir = main.normalized;
        Vector2 perp = new Vector2(-mainDir.y, mainDir.x);

        float angleSum = 0f;
        int angleCount = 0;

        int previousStableSide = 0;

        int step = Mathf.Max(1, pointStepForTurnAnalysis);

        for (int i = step; i < raw.Points.Count; i += step)
        {
            int prevIndex = Mathf.Max(0, i - step);

            Vector2 p0 = raw.Points[prevIndex];
            Vector2 p1 = raw.Points[i];

            Vector2 seg = p1 - p0;
            float segLen = seg.magnitude;

            if (segLen < minSegmentPixelsForTurnAnalysis)
                continue;

            Vector2 segDir = seg / segLen;
            float angle = Vector2.Angle(mainDir, segDir);

            if (angle >= minAngleForTurnSign)
            {
                angleSum += angle;
                angleCount++;
            }

            float sideDist = Vector2.Dot(p1 - start, perp);

            int stableSide = 0;
            if (sideDist > lateralDeadZonePixels) stableSide = 1;
            else if (sideDist < -lateralDeadZonePixels) stableSide = -1;

            if (stableSide != 0)
            {
                if (previousStableSide != 0 && stableSide != previousStableSide)
                    signChanges++;

                previousStableSide = stableSide;
            }
        }

        float avgAngle = angleCount > 0 ? angleSum / angleCount : 0f;

        float anglePenalty = Mathf.InverseLerp(18f, 95f, avgAngle);
        float signChangePenalty = Mathf.Clamp01(signChanges / Mathf.Max(1f, zigZagSignChangeLimit));

        zigZagMetric = Mathf.Clamp01((anglePenalty * 0.45f) + (signChangePenalty * 0.55f));
        return Mathf.Clamp01(1f - zigZagMetric);
    }

    private void Log(ShotSwipeIntent intent, string prefix)
    {
        if (!debugLogs)
            return;

        Debug.Log(
            $"[ShotSwipeAnalyzer] {prefix} | " +
            $"Screen={intent.ScreenWidth}x{intent.ScreenHeight} | Diagonal={intent.ScreenDiagonal:F1} | " +
            $"MinSwipeNeed={intent.MinSwipeLengthNeeded:F1} | MinDirectNeed={intent.MinDirectLengthNeeded:F1} | " +
            $"Valid={intent.IsValid} | Kind={intent.Kind} | Points={intent.PointCount} | " +
            $"Total={intent.TotalLength:F1} | Direct={intent.DirectLength:F1} | Power={intent.Power01:F2} | " +
            $"Curve={intent.CurveSigned01:F2} | CurveAbs={intent.CurveAbs01:F2} | " +
            $"Smooth={intent.Smoothness01:F2} | ZigZag={intent.ZigZag01:F2} | " +
            $"Efficiency={intent.PathEfficiency01:F2} | Malformed={intent.MalformedPath} | " +
            $"SignChanges={intent.DirectionSignChanges} | FinalDir={intent.FinalScreenDirection}");
    }
}