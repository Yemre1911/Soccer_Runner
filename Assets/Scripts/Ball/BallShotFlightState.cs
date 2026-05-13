/*using UnityEngine;

public class BallShotFlightState : IBallState
{
    private readonly ShotBallFlightData _data;

    private float _elapsed;
    private Vector3 _start;
    private Vector3 _target;
    private Vector3 _travelDirection;
    private Vector3 _curveRight;

    public BallShotFlightState(ShotBallFlightData data)
    {
        _data = data;
    }

    public void OnEnter(BallController ball)
    {
        if (_data == null)
        {
            Debug.LogWarning("[BallShotFlightState] Flight data is null.");
            return;
        }

        _elapsed = 0f;

        _start = _data.StartPoint;
        _target = _data.TargetPoint;

        Vector3 flatDirection = _target - _start;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude < 0.001f)
            flatDirection = ball.transform.forward;

        _travelDirection = flatDirection.normalized;

        // Topun uçuş yönüne göre sağ-sol falso yönü.
        _curveRight = Vector3.Cross(Vector3.up, _travelDirection).normalized;

        ball.transform.position = _start;

        Debug.Log(
            $"[BallShotFlightState] Flight started. Start: {_start}, Target: {_target}, Duration: {_data.Duration}, Arc: {_data.ArcHeight}, Curve: {_data.CurveOffset}"
        );
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (_data == null)
            return;

        float dt = _data.UseUnscaledTime ? Time.unscaledDeltaTime : deltaTime;

        float duration = Mathf.Max(0.01f, _data.Duration);

        _elapsed += dt;

        float t = Mathf.Clamp01(_elapsed / duration);

        Vector3 linearPosition = Vector3.Lerp(_start, _target, t);

        // Basit parabola.
        float arc = Mathf.Sin(t * Mathf.PI) * _data.ArcHeight;

        // Falso: başlangıçta 0, ortada maksimum, hedefte tekrar 0.
        float curve = Mathf.Sin(t * Mathf.PI) * _data.CurveOffset;

        Vector3 finalPosition =
            linearPosition +
            Vector3.up * arc +
            _curveRight * curve;

        ball.transform.position = finalPosition;

        // Görsel spin.
        ball.RotateVisualSpin(_data.SpinSpeed, 0f, dt);

        if (t >= 1f)
        {
            ball.transform.position = _target;

            Debug.Log("[BallShotFlightState] Flight completed.");

            // Şimdilik top hedefte kalsın.
            // Sonra burada Goal / Save / Miss result handling bağlayacağız.
            ball.ReleaseBall();
        }
    }

    public void OnExit(BallController ball)
    {
    }
}*/

using UnityEngine;

public class BallShotFlightState : IBallState
{
    private readonly ShotBallFlightData _data;

    private float _elapsed;
    private Vector3 _start;
    private Vector3 _target;
    private Vector3 _travelDirection;
    private Vector3 _curveRight;

    public BallShotFlightState(ShotBallFlightData data)
    {
        _data = data;
    }

    public void OnEnter(BallController ball)
    {
        if (_data == null)
        {
            Debug.LogWarning("[BallShotFlightState] Flight data is null.");
            return;
        }

        _elapsed = 0f;

        _start = _data.StartPoint;
        _target = _data.TargetPoint;

        Vector3 flatDirection = _target - _start;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude < 0.001f)
            flatDirection = ball.transform.forward;

        _travelDirection = flatDirection.normalized;

        // Topun uçuş yönüne göre sağ-sol falso yönü.
        _curveRight = Vector3.Cross(Vector3.up, _travelDirection).normalized;

        ball.transform.position = _start;

        Debug.Log(
            $"[BallShotFlightState] Flight started. Start: {_start}, Target: {_target}, Duration: {_data.Duration}, Arc: {_data.ArcHeight}, Curve: {_data.CurveOffset}"
        );
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (_data == null)
            return;

        float dt = _data.UseUnscaledTime ? Time.unscaledDeltaTime : deltaTime;

        float duration = Mathf.Max(0.01f, _data.Duration);

        _elapsed += dt;

        float t = Mathf.Clamp01(_elapsed / duration);

        Vector3 linearPosition = Vector3.Lerp(_start, _target, t);

        // Basit parabola.
        float arc = Mathf.Sin(t * Mathf.PI) * _data.ArcHeight;

        // Falso: başlangıçta 0, ortada maksimum, hedefte tekrar 0.
        float curve = Mathf.Sin(t * Mathf.PI) * _data.CurveOffset;

        Vector3 finalPosition =
            linearPosition +
            Vector3.up * arc +
            _curveRight * curve;

        ball.transform.position = finalPosition;

        // Görsel spin.
        ball.RotateVisualSpin(_data.SpinSpeed, 0f, dt);

        if (t >= 1f)
        {
            ball.transform.position = _target;

            Debug.Log("[BallShotFlightState] Flight completed. Entering final impact.");

            ball.EnterShotFinalImpact(_data);
        }
    }

    public void OnExit(BallController ball)
    {
    }
}