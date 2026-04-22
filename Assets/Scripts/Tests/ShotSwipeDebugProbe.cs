using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class ShotSwipeDebugProbe : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShotSwipeInputReader inputReader;
    [SerializeField] private ShotSwipeAnalyzer analyzer;
    [SerializeField] private ShotGoalTargetInterpreter targetInterpreter;
    [SerializeField] private ShotResolver resolver;
    [SerializeField] private ShotSwipeVisualizer visualizer;
    [SerializeField] private ShotSequenceController shotSequenceController;

    [Header("Debug")]
    [SerializeField] private bool printToConsole = true;
    [SerializeField] private bool showOnScreen = true;
    [SerializeField] private bool verboseLogBlocks = true;
    [SerializeField] private int maxEntries = 8;

    private readonly Queue<string> _entries = new Queue<string>();

    private void Awake()
    {
        if (!inputReader) inputReader = FindFirstObjectByType<ShotSwipeInputReader>();
        if (!analyzer) analyzer = FindFirstObjectByType<ShotSwipeAnalyzer>();
        if (!targetInterpreter) targetInterpreter = FindFirstObjectByType<ShotGoalTargetInterpreter>();
        if (!resolver) resolver = FindFirstObjectByType<ShotResolver>();
        if (!visualizer) visualizer = FindFirstObjectByType<ShotSwipeVisualizer>();
        if (!shotSequenceController) shotSequenceController = FindFirstObjectByType<ShotSequenceController>();
    }

    private void OnEnable()
    {
        if (inputReader != null)
            inputReader.OnSwipeCompleted += HandleSwipeCompleted;
    }

    private void OnDisable()
    {
        if (inputReader != null)
            inputReader.OnSwipeCompleted -= HandleSwipeCompleted;
    }

    private void HandleSwipeCompleted(RawSwipeData raw)
    {
        ShotSwipeIntent intent = analyzer != null ? analyzer.Analyze(raw) : null;
        Transform goal = shotSequenceController != null ? GetGoalTransformFromSequence() : null;
        ShotGoalTarget target = targetInterpreter != null && intent != null ? targetInterpreter.Interpret(intent, goal) : null;
        ShotDecisionResult decision = resolver != null && intent != null ? resolver.Resolve(intent, target, goal) : null;

        if (visualizer != null && raw != null)
            visualizer.ShowResolvedSwipe(raw, target);

        string log = BuildLog(raw, intent, target, decision);

        if (printToConsole)
            Debug.Log(log);

        AddEntry(log);
    }

    private string BuildLog(RawSwipeData raw, ShotSwipeIntent intent, ShotGoalTarget target, ShotDecisionResult decision)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== SHOT SWIPE DEBUG ===");

        if (raw != null)
        {
            sb.AppendLine($"RAW.Points                 : {(raw.Points != null ? raw.Points.Count : 0)}");
            sb.AppendLine($"RAW.Duration               : {raw.Duration:F3}");
            sb.AppendLine($"RAW.Start                  : {raw.StartPoint}");
            sb.AppendLine($"RAW.End                    : {raw.EndPoint}");
            sb.AppendLine($"RAW.TimeoutEnd             : {raw.EndedByTimeout}");
        }

        if (intent != null)
        {
            sb.AppendLine($"INTENT.Valid               : {intent.IsValid}");
            sb.AppendLine($"INTENT.Kind                : {intent.Kind}");
            sb.AppendLine($"INTENT.MalformedPath       : {intent.MalformedPath}");
            sb.AppendLine($"INTENT.ScreenStart         : {intent.ScreenStart}");
            sb.AppendLine($"INTENT.ScreenEnd           : {intent.ScreenEnd}");
            sb.AppendLine($"INTENT.FinalDir            : {intent.FinalScreenDirection}");
            sb.AppendLine($"INTENT.TotalLength         : {intent.TotalLength:F1}");
            sb.AppendLine($"INTENT.DirectLength        : {intent.DirectLength:F1}");
            sb.AppendLine($"INTENT.Power01             : {intent.Power01:F2}");
            sb.AppendLine($"INTENT.CurveSigned01       : {intent.CurveSigned01:F2}");
            sb.AppendLine($"INTENT.CurveAbs01          : {intent.CurveAbs01:F2}");
            sb.AppendLine($"INTENT.Smoothness01        : {intent.Smoothness01:F2}");
            sb.AppendLine($"INTENT.ZigZag01            : {intent.ZigZag01:F2}");
            sb.AppendLine($"INTENT.PathEfficiency01    : {intent.PathEfficiency01:F2}");
            sb.AppendLine($"INTENT.SignChanges         : {intent.DirectionSignChanges}");
            sb.AppendLine($"INTENT.TooShort            : {intent.TooShort}");
            sb.AppendLine($"INTENT.TooLong             : {intent.TooLong}");
            sb.AppendLine($"INTENT.Timeout             : {intent.EndedByTimeout}");

            sb.AppendLine($"INTENT.Screen              : {intent.ScreenWidth}x{intent.ScreenHeight}");
            sb.AppendLine($"INTENT.Diagonal            : {intent.ScreenDiagonal:F1}");
            sb.AppendLine($"INTENT.MinSwipeNeed        : {intent.MinSwipeLengthNeeded:F1}");
            sb.AppendLine($"INTENT.MinDirectNeed       : {intent.MinDirectLengthNeeded:F1}");
        }

        if (target != null)
        {
            sb.AppendLine($"TARGET.InitialHit          : {target.InitialHitType}");
            sb.AppendLine($"TARGET.FinalHit            : {target.FinalHitType}");
            sb.AppendLine($"TARGET.InsideGoal          : {target.InsideGoal}");
            sb.AppendLine($"TARGET.InitialScreen       : {target.InitialScreenPoint}");
            sb.AppendLine($"TARGET.FinalScreen         : {target.FinalScreenPoint}");
            sb.AppendLine($"TARGET.InitialWorld        : {target.InitialWorldPoint}");
            sb.AppendLine($"TARGET.FinalWorld          : {target.WorldPoint}");
            sb.AppendLine($"TARGET.HitGroundInitially  : {target.HitGroundInitially}");
            sb.AppendLine($"TARGET.UsedAutoExtension   : {target.UsedAutoExtension}");
            sb.AppendLine($"TARGET.ExtensionStillGround: {target.ExtensionStillOnGround}");
            sb.AppendLine($"TARGET.ExtensionSteps      : {target.AutoExtendSteps}");
            sb.AppendLine($"TARGET.ExtensionPixels     : {target.AutoExtendDistancePixels:F1}");
            sb.AppendLine($"TARGET.AutoExtHit          : {target.AutoExtendedResolvedHitType}");
            sb.AppendLine($"TARGET.TargetNorm          : {target.Normalized}");
            sb.AppendLine($"TARGET.Centrality01        : {target.Centrality01:F2}");
            sb.AppendLine($"TARGET.DebugReason         : {target.DebugReason}");
        }
        else
        {
            sb.AppendLine("TARGET                     : NULL");
        }

        if (decision != null)
        {
            sb.AppendLine($"RESULT.Outcome             : {decision.Outcome}");
            sb.AppendLine($"RESULT.Code                : {decision.Code}");
            sb.AppendLine($"RESULT.IsGoal              : {decision.IsGoal}");
            sb.AppendLine($"RESULT.Reason              : {decision.Reason}");
        }
        else
        {
            sb.AppendLine("RESULT                     : NULL");
        }

        if (verboseLogBlocks)
            sb.AppendLine("===========================");

        return sb.ToString();
    }

    private Transform GetGoalTransformFromSequence()
    {
        FieldInfo field = typeof(ShotSequenceController).GetField("_goalTransform", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
            return null;

        return field.GetValue(shotSequenceController) as Transform;
    }

    private void AddEntry(string text)
    {
        _entries.Enqueue(text);

        while (_entries.Count > maxEntries)
            _entries.Dequeue();
    }

    private void OnGUI()
    {
        if (!showOnScreen || _entries.Count == 0)
            return;

        GUILayout.BeginArea(new Rect(20, 20, 760, Screen.height - 40), GUI.skin.box);

        foreach (string entry in _entries)
        {
            GUILayout.Label(entry);
            GUILayout.Space(8);
        }

        GUILayout.EndArea();
    }
}