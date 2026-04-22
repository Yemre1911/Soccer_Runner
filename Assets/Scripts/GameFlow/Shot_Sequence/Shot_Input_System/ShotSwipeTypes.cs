/*using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShotSwipeKind
{
    Invalid,
    Straight,
    Curve,
    OverCurve,
    ZigZag
}

public enum ShotOutcomeType
{
    Goal,
    Saved,
    MissOut,
    MissWide,
    TimeoutOut
}

public enum ShotHitType
{
    None,
    Goal,
    KeeperFront,
    KeeperWide,
    Ground
}

[Serializable]
public class RawSwipeData
{
    public List<Vector2> Points = new List<Vector2>();
    public float StartRealtime;
    public float EndRealtime;
    public bool EndedByTimeout;

    public float Duration => Mathf.Max(0f, EndRealtime - StartRealtime);

    public Vector2 StartPoint => (Points == null || Points.Count == 0) ? Vector2.zero : Points[0];
    public Vector2 EndPoint => (Points == null || Points.Count == 0) ? Vector2.zero : Points[Points.Count - 1];

    public RawSwipeData Clone()
    {
        return new RawSwipeData
        {
            Points = new List<Vector2>(Points),
            StartRealtime = StartRealtime,
            EndRealtime = EndRealtime,
            EndedByTimeout = EndedByTimeout
        };
    }
}

[Serializable]
public class ShotSwipeIntent
{
    public bool IsValid;
    public bool TooShort;
    public bool TooLong; // compatibility için bırakıldı, artık false kalacak
    public bool EndedByTimeout;

    public ShotSwipeKind Kind;

    public float TotalLength;
    public float DirectLength;
    public float Power01;
    public float CurveSigned01;
    public float CurveAbs01;
    public float Quality01;
    public float Smoothness01;
    public float ZigZag01;

    public Vector2 ScreenStart;
    public Vector2 ScreenEnd;
    public Vector2 FinalScreenDirection;
    public int PointCount;
    public int DirectionSignChanges;

    // Debug / responsive threshold bilgileri
    public int ScreenWidth;
    public int ScreenHeight;
    public float ScreenDiagonal;
    public float MinSwipeLengthNeeded;
    public float MinDirectLengthNeeded;

    public bool IsCurveAcceptable =>
        IsValid &&
        Kind != ShotSwipeKind.OverCurve &&
        Kind != ShotSwipeKind.ZigZag;
}

[Serializable]
public class ShotGoalTarget
{
    public bool InsideGoal;
    public Vector2 Normalized;
    public float Centrality01;
    public Vector3 WorldPoint;

    public ShotHitType InitialHitType;
    public ShotHitType FinalHitType;
    public ShotHitType AutoExtendedResolvedHitType;

    public bool HitGroundInitially;
    public bool UsedAutoExtension;
    public bool ExtensionStillOnGround;

    public Vector2 InitialScreenPoint;
    public Vector2 FinalScreenPoint;
    public Vector2 ExtensionDirection;
    public Vector2 AutoExtendedScreenPoint;

    public Vector3 InitialWorldPoint;
    public Vector3 AutoExtendedWorldPoint;

    public int AutoExtendSteps;
    public float AutoExtendDistancePixels;

    public string DebugReason;
}

[Serializable]
public class ShotDecisionResult
{
    public ShotOutcomeType Outcome;
    public bool IsGoal;
    public string Reason;
    public ShotSwipeIntent Intent;
    public ShotGoalTarget Target;
}*/

using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShotSwipeKind
{
    Invalid,
    Straight,
    Curve,
    OverCurve,
    ZigZag
}

public enum ShotOutcomeType
{
    Goal,
    Saved,
    MissOut,
    MissWide,
    TimeoutOut
}

public enum ShotHitType
{
    None,
    Goal,
    KeeperFront,
    KeeperWide,
    Ground
}

public enum ShotDecisionCode
{
    None = 0,

    // invalid / bad input
    InvalidSwipe = 10,
    Timeout = 11,
    OverCurveOut = 12,
    ZigZagOut = 13,
    MalformedOut = 14,

    // misses
    MissedNoTarget = 20,
    GroundedOut = 21,

    // saves
    SavedByKeeperFront = 30,
    SavedByKeeperWideLowCurve = 31,
    SavedGroundedGoalLane = 32,
    SavedGroundedKeeperLane = 33,

    // goals
    GoalDirect = 40,
    GoalWideKeeperCurveBeat = 41
}

[Serializable]
public class RawSwipeData
{
    public List<Vector2> Points = new List<Vector2>();
    public float StartRealtime;
    public float EndRealtime;
    public bool EndedByTimeout;

    public float Duration => Mathf.Max(0f, EndRealtime - StartRealtime);

    public Vector2 StartPoint => (Points == null || Points.Count == 0) ? Vector2.zero : Points[0];
    public Vector2 EndPoint => (Points == null || Points.Count == 0) ? Vector2.zero : Points[Points.Count - 1];

    public RawSwipeData Clone()
    {
        return new RawSwipeData
        {
            Points = new List<Vector2>(Points),
            StartRealtime = StartRealtime,
            EndRealtime = EndRealtime,
            EndedByTimeout = EndedByTimeout
        };
    }
}

[Serializable]
public class ShotSwipeIntent
{
    public bool IsValid;
    public bool TooShort;
    public bool TooLong; // compatibility için tutuldu, artık false
    public bool EndedByTimeout;
    public bool MalformedPath;

    public ShotSwipeKind Kind;

    public float TotalLength;
    public float DirectLength;
    public float Power01;
    public float CurveSigned01;
    public float CurveAbs01;
    public float Quality01;
    public float Smoothness01;
    public float ZigZag01;
    public float PathEfficiency01;

    public Vector2 ScreenStart;
    public Vector2 ScreenEnd;
    public Vector2 FinalScreenDirection;
    public int PointCount;
    public int DirectionSignChanges;

    // responsive threshold debug
    public int ScreenWidth;
    public int ScreenHeight;
    public float ScreenDiagonal;
    public float MinSwipeLengthNeeded;
    public float MinDirectLengthNeeded;

    public bool IsCurveAcceptable =>
        IsValid &&
        !MalformedPath &&
        Kind != ShotSwipeKind.OverCurve &&
        Kind != ShotSwipeKind.ZigZag;
}

[Serializable]
public class ShotGoalTarget
{
    public bool InsideGoal;
    public Vector2 Normalized;
    public float Centrality01;
    public Vector3 WorldPoint;

    public ShotHitType InitialHitType;
    public ShotHitType FinalHitType;
    public ShotHitType AutoExtendedResolvedHitType;

    public bool HitGroundInitially;
    public bool UsedAutoExtension;
    public bool ExtensionStillOnGround;

    public Vector2 InitialScreenPoint;
    public Vector2 FinalScreenPoint;
    public Vector2 ExtensionDirection;
    public Vector2 AutoExtendedScreenPoint;

    public Vector3 InitialWorldPoint;
    public Vector3 AutoExtendedWorldPoint;

    public int AutoExtendSteps;
    public float AutoExtendDistancePixels;

    public string DebugReason;
}

[Serializable]
public class ShotDecisionResult
{
    public ShotOutcomeType Outcome;
    public ShotDecisionCode Code;
    public bool IsGoal;
    public string Reason;
    public ShotSwipeIntent Intent;
    public ShotGoalTarget Target;
}