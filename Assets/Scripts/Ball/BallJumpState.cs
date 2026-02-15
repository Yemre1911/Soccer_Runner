using UnityEngine;

public class BallJumpState : IBallState
{
    private PlayerLateralMovement _lateralScript;
    private PlayerVerticalMovement _verticalScript; // Zıplama sürelerini almak için

    private Vector3 _velocity; // SmoothDamp için
    private float _verticalVelocity;
    private float _gravity;
    private float _jumpVelocity;
    
    // Topun karakterden ne kadar daha yükseğe çıkacağı (Juice Effect)
    private float _extraJumpHeight = 1.5f; 
    private bool _isQuickDropping;

    public void OnEnter(BallController ball)
    {
        if (ball.PlayerSocket != null)
        {
            _lateralScript = ball.PlayerSocket.GetComponentInParent<PlayerLateralMovement>();
            _verticalScript = ball.PlayerSocket.GetComponentInParent<PlayerVerticalMovement>();
        }

        // --- MATEMATİKSEL HESAPLAMA ---
        // Karakterin havada kalma süresini (Time to Apex) alıyoruz.
        // Topun da aynı sürede inmesi lazım ama daha yükseğe çıkacak.
        float timeToApex = _verticalScript.TimeToJumpApex;
        float totalHeight = _verticalScript.JumpHeight + _extraJumpHeight;

        // Formül: g = 2h / t^2
        _gravity = -(2 * totalHeight) / Mathf.Pow(timeToApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToApex;

        // İlk fırlatma
        _verticalVelocity = _jumpVelocity;
        _isQuickDropping = false;
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (_lateralScript == null) return;

        // 1. YER ÇEKİMİ VE DÜŞÜŞ
        if (_isQuickDropping)
        {
            // Player süzülürken top mermi gibi insin
            _verticalVelocity += _gravity * 5f * deltaTime; 
        }
        else
        {
            _verticalVelocity += _gravity * deltaTime;
        }

        // 2. POZİSYON HESAPLAMA
        Vector3 targetPos = ball.transform.position;
        
        // Y Ekseni (Zıplama)
        targetPos.y += _verticalVelocity * deltaTime;

        // X Ekseni (Karakter nereye gidiyorsa oraya git)
        targetPos.x = Mathf.SmoothDamp(targetPos.x, _lateralScript.TargetX, ref _velocity.x, 0.1f);

        // Z Ekseni (Havadayken topu karakterin tam önüne/altına sabitle, ileri geri yapmasın)
        // Karakterin Z'si + biraz öne (0.5f)
        float targetZ = ball.PlayerSocket.position.z + 0.5f;
        targetPos.z = Mathf.SmoothDamp(targetPos.z, targetZ, ref _velocity.z, 0.1f);

        // Pozisyonu uygula
        ball.transform.position = targetPos;

        // 3. ZEMİN KONTROLÜ (State Çıkışı)
        // Eğer top yere indiyse tekrar Dribble moduna geç
        if (ball.transform.position.y <= -0.55f && _verticalVelocity < 0) // -0.55f senin baseHeight değerin
        {
            // Topu tam zemine oturt
            Vector3 landPos = ball.transform.position;
            landPos.y = -0.55f;
            ball.transform.position = landPos;

            ball.ChangeState(new DribbleState());
        }

        // 4. GÖRSEL DÖNME (Havadayken öne doğru dönsün)
        ball.transform.Rotate(Vector3.right * 1000f * deltaTime, Space.World);
    }

    public void OnExit(BallController ball)
    {
        // Yere inince efekt, toz bulutu vs. burada yapılabilir.
    }

    // Karakterden gelen "Hızlı İn" emrini dinlemek için
    public void SetQuickDrop()
    {
        if (_verticalVelocity > 0) _verticalVelocity = 0; // Yükselmeyi kes
        _isQuickDropping = true;
    }
}