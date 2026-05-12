using System;
using UnityEngine;

[Serializable]
public class ShotBallFlightData
{
    public Vector3 StartPoint;
    public Vector3 TargetPoint;

    public float Duration;
    public float ArcHeight;
    public float CurveOffset;
    public float SpinSpeed;

    public bool UseUnscaledTime;

    public ShotDecisionResult Decision;
}