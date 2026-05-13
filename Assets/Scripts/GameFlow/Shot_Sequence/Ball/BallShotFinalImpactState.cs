using UnityEngine;

public class BallShotFinalImpactState : IBallState
{
    private enum FinalImpactMode
    {
        None,
        GoalContinue,
        OutContinue,
        CleanCatch,
        Deflect
    }

    private readonly ShotBallFlightData _data;

    private FinalImpactMode _mode;

    private float _elapsed;

    private Vector3 _start;
    private Vector3 _target;
    private Vector3 _direction;

    private bool _catchSnapFinished;

    public BallShotFinalImpactState(ShotBallFlightData data)
    {
        _data = data;
    }

    public void OnEnter(BallController ball)
    {
        _elapsed = 0f;
        _catchSnapFinished = false;

        _start = ball.transform.position;
        _mode = ResolveMode(_data);

        switch (_mode)
        {
            case FinalImpactMode.GoalContinue:
                BeginGoalContinue(ball);
                break;

            case FinalImpactMode.OutContinue:
                BeginOutContinue(ball);
                break;

            case FinalImpactMode.CleanCatch:
                BeginCleanCatch(ball);
                break;

            case FinalImpactMode.Deflect:
                BeginDeflect(ball);
                break;

            default:
                if (ball.DebugFinalImpact)
                    Debug.LogWarning("[BallShotFinalImpactState] Unknown impact mode. Releasing ball.");

                ball.ReleaseBall();
                break;
        }

        if (ball.DebugFinalImpact)
        {
            Debug.Log(
                $"[BallShotFinalImpactState] Enter | " +
                $"Code={_data?.Decision?.Code} | " +
                $"Mode={_mode} | " +
                $"Start={_start} | " +
                $"Target={_target}"
            );
        }
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (_data == null)
            return;

        float dt = ball.FinalImpactUseUnscaledTime ? Time.unscaledDeltaTime : deltaTime;

        switch (_mode)
        {
            case FinalImpactMode.GoalContinue:
                UpdateSecondaryFlight(
                    ball,
                    dt,
                    ball.GoalContinueDuration,
                    ball.GoalContinueArcHeight,
                    _data.SpinSpeed,
                    releaseWhenFinished: true
                );
                break;

            case FinalImpactMode.OutContinue:
                UpdateSecondaryFlight(
                    ball,
                    dt,
                    ball.OutContinueDuration,
                    ball.OutContinueArcHeight,
                    _data.SpinSpeed,
                    releaseWhenFinished: true
                );
                break;

            case FinalImpactMode.CleanCatch:
                UpdateCleanCatch(ball, dt);
                break;

            case FinalImpactMode.Deflect:
                UpdateSecondaryFlight(
                    ball,
                    dt,
                    ball.DeflectDuration,
                    ball.DeflectArcHeight,
                    ball.DeflectSpinSpeed,
                    releaseWhenFinished: true
                );
                break;
        }
    }

    public void OnExit(BallController ball)
    {
    }

    private FinalImpactMode ResolveMode(ShotBallFlightData data)
    {
        if (data == null || data.Decision == null)
            return FinalImpactMode.OutContinue;

        switch (data.Decision.Code)
        {
            case ShotDecisionCode.GoalDirect:
            case ShotDecisionCode.GoalWideKeeperCurveBeat:
                return FinalImpactMode.GoalContinue;

            case ShotDecisionCode.SavedByKeeperUpper:
            case ShotDecisionCode.SavedByKeeperFront:
            case ShotDecisionCode.SavedGroundedKeeperLane:
                return FinalImpactMode.CleanCatch;

            case ShotDecisionCode.SavedByKeeperWideLowCurve:
            case ShotDecisionCode.SavedGroundedGoalLane:
                return FinalImpactMode.Deflect;

            case ShotDecisionCode.MissedNoTarget:
            case ShotDecisionCode.GroundedOut:
            case ShotDecisionCode.InvalidSwipe:
            case ShotDecisionCode.Timeout:
            case ShotDecisionCode.OverCurveOut:
            case ShotDecisionCode.ZigZagOut:
            case ShotDecisionCode.MalformedOut:
            case ShotDecisionCode.None:
            default:
                return FinalImpactMode.OutContinue;
        }
    }

    private void BeginGoalContinue(BallController ball)
    {
        _direction = ResolveFlatFlightDirection(ball);

        _target = _start + _direction * Mathf.Max(0.1f, ball.GoalContinueDistance);

        // Top kalenin içine giderken zemine yapışmış gibi görünmesin.
        _target.y = Mathf.Max(_target.y, 0.35f);
    }

    private void BeginOutContinue(BallController ball)
    {
        _direction = ResolveFlatFlightDirection(ball);

        _target = _start + _direction * Mathf.Max(0.1f, ball.OutContinueDistance);

        // Out/miss sonrası top hedefi geçsin ama zemine saplanmış gibi görünmesin.
        _target.y = Mathf.Max(_target.y, 0.25f);
    }

    private void BeginCleanCatch(BallController ball)
    {
        if (ball.GlovePoint == null)
        {
            Debug.LogWarning("[BallShotFinalImpactState] GlovePoint is missing. Ball will stay at impact point.");
            _target = _start;
            _catchSnapFinished = true;
            return;
        }

        AnimatedGloveSocketFollower gloveFollower = ball.GlovePoint.GetComponent<AnimatedGloveSocketFollower>();

        if (gloveFollower != null)
        {
            gloveFollower.SetBallReference(ball.transform);

            // AutoByBallSide açık kalırsa top glove'a yaklaşırken sağ-sol el arasında flicker yapabilir.
            // Bu yüzden catch başladığı anda el bir kere seçilir ve kilitlenir.
            gloveFollower.LockHandByBallSide();
        }

        _target = ball.GlovePoint.position;
    }

    private void BeginDeflect(BallController ball)
    {
        Vector3 incomingDirection = ResolveIncomingDirectionAtImpact(ball);

        if (incomingDirection.sqrMagnitude < 0.001f)
            incomingDirection = ResolveFlatFlightDirection(ball);

        incomingDirection.Normalize();

        Vector3 surfaceNormal = ResolveDeflectSurfaceNormal(ball, incomingDirection);

        if (surfaceNormal.sqrMagnitude < 0.001f)
            surfaceNormal = -ResolveFlatFlightDirection(ball);

        surfaceNormal.Normalize();

        // Kalecinin eli/gövdesi dikey yüzey gibi davranır.
        // Reflect, gelen yönü bu yüzey normaline göre gerçekçi şekilde sektirir.
        Vector3 reflectedDirection = Vector3.Reflect(incomingDirection, surfaceNormal);

        if (reflectedDirection.sqrMagnitude < 0.001f)
            reflectedDirection = -ResolveFlatFlightDirection(ball);

        reflectedDirection.Normalize();

        _direction = reflectedDirection;

        _target = _start + _direction * Mathf.Max(0.1f, ball.DeflectDistance);

        // Deflect yönü bazen aşağı bileşen taşıyabilir.
        // Topun sekme sonrası zemine gömülmesini engelliyoruz.
        _target.y = Mathf.Max(_target.y, 0.45f);

        if (ball.DebugFinalImpact)
        {
            Debug.Log(
                $"[BallShotFinalImpactState] Deflect Wall Reflection | " +
                $"Incoming={incomingDirection}, " +
                $"SurfaceNormal={surfaceNormal}, " +
                $"Reflected={reflectedDirection}, " +
                $"Target={_target}"
            );
        }
    }

    private void UpdateSecondaryFlight(
        BallController ball,
        float dt,
        float duration,
        float arcHeight,
        float spinSpeed,
        bool releaseWhenFinished)
    {
        _elapsed += dt;

        float safeDuration = Mathf.Max(0.01f, duration);
        float t = Mathf.Clamp01(_elapsed / safeDuration);

        Vector3 linear = Vector3.Lerp(_start, _target, t);
        float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

        ball.transform.position = linear + Vector3.up * arc;

        ball.RotateVisualSpin(spinSpeed, 0f, dt);

        if (t >= 1f)
        {
            ball.transform.position = _target;

            if (ball.DebugFinalImpact)
                Debug.Log($"[BallShotFinalImpactState] Secondary flight completed. Mode={_mode}");

            if (releaseWhenFinished)
                ball.ReleaseBall();
        }
    }

    private void UpdateCleanCatch(BallController ball, float dt)
    {
        if (ball.GlovePoint == null)
            return;

        if (!_catchSnapFinished)
        {
            _elapsed += dt;

            float duration = Mathf.Max(0.01f, ball.CatchSnapDuration);
            float t = Mathf.Clamp01(_elapsed / duration);

            ball.transform.position = Vector3.Lerp(_start, ball.GlovePoint.position, t);

            // Clean catch sırasında spin durmalı.
            // Bu yüzden burada RotateVisualSpin çağırmıyoruz.

            if (t >= 1f)
            {
                _catchSnapFinished = true;

                if (ball.DebugFinalImpact)
                    Debug.Log("[BallShotFinalImpactState] Catch snap finished. Following GlovePoint without spin.");
            }

            return;
        }

        // Parent yapmıyoruz.
        // Direkt world position/rotation takip ederek rig-scale sorunlarını azaltıyoruz.
        ball.transform.position = ball.GlovePoint.position;
        ball.transform.rotation = ball.GlovePoint.rotation;
    }

    private Vector3 ResolveIncomingDirectionAtImpact(BallController ball)
    {
        if (_data == null)
            return ResolveFlatFlightDirection(ball);

        float duration = Mathf.Max(0.01f, _data.Duration);

        // Ana uçuşun lineer velocity'si.
        Vector3 linearVelocity = (_data.TargetPoint - _data.StartPoint) / duration;

        // İlk uçuşta arc şöyleydi:
        // arc = Sin(t * PI) * ArcHeight
        // Hedefe varırken t=1, cos(PI)=-1 olduğu için dikey hız aşağı yönlüdür.
        float verticalVelocityAtImpact = -Mathf.PI * _data.ArcHeight / duration;

        Vector3 incomingVelocity = linearVelocity + Vector3.up * verticalVelocityAtImpact;

        if (incomingVelocity.sqrMagnitude < 0.001f)
            return ResolveFlatFlightDirection(ball);

        return incomingVelocity.normalized;
    }

    private Vector3 ResolveDeflectSurfaceNormal(BallController ball, Vector3 incomingDirection)
    {
        // Kaleci eli/gövdesi dikey bir yüzey gibi davranır.
        // Bu yüzden normal, topun geliş yönünün yatay tersine bakmalı.
        //
        // Önemli:
        // - Vector3.up kullanırsak top yerden sekiyormuş gibi olur.
        // - PlayerReference'a doğru normal verirsek top çoğu durumda dümdüz geri döner.
        // - Burada normal, incomingDirection'ın yatay tersidir.

        Vector3 normal = -incomingDirection;
        normal.y = 0f;

        if (normal.sqrMagnitude < 0.001f)
        {
            // Fallback:
            // Incoming tamamen dikeyse, player referansından ters yönde yaklaşık bir normal çıkar.
            if (ball.PlayerReference != null)
            {
                normal = _start - ball.PlayerReference.position;
                normal.y = 0f;
            }

            if (normal.sqrMagnitude < 0.001f)
                normal = -ResolveFlatFlightDirection(ball);
        }

        normal.Normalize();

        // Hafif yukarı tilt.
        // El yüzeyi tam dikey değil, biraz yukarı bakıyormuş gibi davranır.
        // Bu topun save sonrası daha görünür/havada sekmesini sağlar.
        //
        // 0.20 - 0.45 arası dene.
        // Fazla yükselirse azalt.
        // Çok yerde kalırsa artır.
        normal = (normal + Vector3.up * 0.25f).normalized;

        return normal;
    }

    private Vector3 ResolveFlatFlightDirection(BallController ball)
    {
        if (_data != null)
        {
            Vector3 fromOriginalStart = _data.TargetPoint - _data.StartPoint;
            fromOriginalStart.y = 0f;

            if (fromOriginalStart.sqrMagnitude > 0.001f)
                return fromOriginalStart.normalized;
        }

        Vector3 forward = ball.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        return forward.normalized;
    }
}