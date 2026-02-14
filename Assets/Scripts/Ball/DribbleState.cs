/*using UnityEngine;

public class DribbleState : IBallState
{
    private Vector3 _velocity;
    private PlayerLateralMovement _playerScript;

    // --- AYARLAR ---
    private float _ballRadiusHeight = -0.55f; // Topun yerden yüksekliği
    
    // YENİ: Top Sürme Ritmi
    private float _dribbleFrequency = 10f; // Ayak hızı (Saniyede kaç tur)
    private float _dribbleAmplitude = 2f; // Topun ayaktan ne kadar açılacağı (Mesafe)

    public void OnEnter(BallController ball)
    {
        if (ball.PlayerSocket != null)
        {
            _playerScript = ball.PlayerSocket.GetComponentInParent<PlayerLateralMovement>();
        }
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (ball.PlayerSocket == null || _playerScript == null) return;

        // 1. X EKSENİ (Sağ/Sol) - (Eski mantık aynen duruyor)
        float targetX = _playerScript.TargetX;
        float ballSmoothTime = _playerScript.MovementSmoothTime * 0.5f;

        // 2. Z EKSENİ (İleri/Geri) - YENİ KISIM: SİNÜS DALGASI
        // Mathf.Sin -> -1 ile 1 arasında gider gelir.
        // Biz topun arkaya (-1) gitmesini istemiyoruz, sadece ileri (0 ile 1) gitmesini istiyoruz.
        // Formül: (Sin + 1) / 2 => Sonuç her zaman 0 ile 1 arasındadır.
        
        float cycle = (Mathf.Sin(Time.time * _dribbleFrequency) + 1f) * 0.5f; 
        
        // Bu cycle değerini mesafeyle çarpıp Socket'in Z'sine ekliyoruz.
        // Böylece top Socket (0) ile Socket + Amplitude (0.8) arasında gidip gelir.
        float targetZ = ball.PlayerSocket.position.z + (cycle * _dribbleAmplitude);

        // 3. HEDEF VEKTÖRÜ OLUŞTUR
        Vector3 targetPosition = new Vector3(targetX, _ballRadiusHeight, targetZ);

        // 4. HAREKETİ UYGULA
        ball.transform.position = Vector3.SmoothDamp(
            ball.transform.position,
            targetPosition,
            ref _velocity,
            ballSmoothTime, 
            50f, 
            deltaTime
        );

        // 5. GÖRSEL DÖNME (Aynen duruyor)
        float runSpeed = 1000f * deltaTime; 
        ball.transform.Rotate(Vector3.right * runSpeed, Space.World);
        float tilt = _velocity.x * 2f; 
        ball.transform.Rotate(Vector3.forward * -tilt * deltaTime, Space.World);
    }

    public void OnExit(BallController ball) { }
}*/


using UnityEngine;

public class DribbleState : IBallState
{
    private Vector3 _velocity; // SmoothDamp referansı
    private PlayerLateralMovement _playerScript;

    // --- AYARLAR ---
    [Header("Yükseklik & Kavis")]
    private float _baseHeight = -0.55f; // Topun yerdeki yüksekliği
    private float _arcHeight = 0.35f;  // İleri giderken ne kadar yükselsin?

    [Header("Dribble Ritmi")]
    private float _dribbleFrequency = 10f; // Ayak hızı
    private float _dribbleAmplitude = 2f; // Top ne kadar öne açılsın

    [Header("Akışkanlık (Game Feel)")]
    private float _lateralSmoothTime = 0.05f; // Yanlara kayma yumuşaklığı (Şşlaaapks hissi)
    private float _returnToFootSpeed = 20f;   // Yana kaçarken topu geri çekme hızı

    public void OnEnter(BallController ball)
    {
        if (ball.PlayerSocket != null)
        {
            _playerScript = ball.PlayerSocket.GetComponentInParent<PlayerLateralMovement>();
        }
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (ball.PlayerSocket == null || _playerScript == null) return;

        // 1. DURUM ANALİZİ: Şuan yana mı gidiyoruz, düz mü?
        // Topun şu anki X'i ile Hedef X arasındaki farka bak.
        float xDifference = Mathf.Abs(ball.transform.position.x - _playerScript.TargetX);
        
        // Eğer fark 0.1f'den büyükse, karakter şerit değiştiriyordur.
        bool isChangingLane = xDifference > 0.1f;

        // ---------------------------------------------------------

        // 2. Z EKSENİ (İleri/Geri ve Geri Çekme)
        float targetZ;
        float cycle = 0f;

        if (isChangingLane)
        {
            // KURAL 2: Sağa/Sola gidiyorsak, topu hemen ayağa (Socket'e) çek!
            // Amplitude'u 0 gibi düşünüyoruz.
            targetZ = ball.PlayerSocket.position.z;
            
            // Cycle 0 olsun ki top yere insin (Kavis yapmasın)
            cycle = 0f; 
        }
        else
        {
            // Düz gidiyorsak ritmik sürmeye devam et
            // (Sin + 1) * 0.5 formülü bize 0 ile 1 arası yumuşak bir değer verir.
            cycle = (Mathf.Sin(Time.time * _dribbleFrequency) + 1f) * 0.5f;
            targetZ = ball.PlayerSocket.position.z + (cycle * _dribbleAmplitude);
        }

        // ---------------------------------------------------------

        // 3. Y EKSENİ (Kavisli Yükseliş - KURAL 1)
        // Cycle 0 ise (ayakta) yükseklik _baseHeight.
        // Cycle 1 ise (en uçta) yükseklik _baseHeight + _arcHeight.
        // Bu basit toplama işlemi, top ileri gittikçe yükselmesini sağlar.
        float targetY = _baseHeight + (cycle * _arcHeight);

        // ---------------------------------------------------------

        // 4. X EKSENİ (Yanal Hareket - KURAL 3)
        // Karakterin hedefi neresiyse oraya git.
        float targetX = _playerScript.TargetX;

        // 5. HAREKETİ UYGULA (SmoothDamp ile Şşlaaapks Hissi)
        
        // Yanlara giderken kullanacağımız SmoothTime ayarı.
        // Eğer şerit değiştiriyorsak biraz daha yumuşak (_lateralSmoothTime),
        // Düz gidiyorsak karakterin hızına bağlı (daha snappy).
        float currentSmoothTime = isChangingLane ? _lateralSmoothTime : 0.05f;

        Vector3 targetPosition = new Vector3(targetX, targetY, targetZ);

        // Z ekseni için özel bir durum: Yana geçerken Z'yi çok hızlı geri çekmeliyiz.
        // Bu yüzden SmoothDamp'ın "MaxSpeed" parametresini duruma göre değiştiriyoruz.
        float maxSpeed = isChangingLane ? _returnToFootSpeed * 2f : 50f;

        ball.transform.position = Vector3.SmoothDamp(
            ball.transform.position,
            targetPosition,
            ref _velocity,
            currentSmoothTime,
            maxSpeed,
            deltaTime
        );

        // ---------------------------------------------------------

        // 6. GÖRSEL DÖNME (ROTATION)
        RotateBall(ball.transform, deltaTime, _velocity);
    }

    private void RotateBall(Transform t, float dt, Vector3 velocity)
    {
        // İleri doğru yuvarlanma (Hıza bağlı)
        float runSpeed = 800f * dt; 
        t.Rotate(Vector3.right * runSpeed, Space.World);

        // Sağa sola yatma (Banking) - Dönüş hissiyatını artırır
        float tilt = velocity.x * 3f; 
        t.Rotate(Vector3.forward * -tilt * dt, Space.World);
    }
    
    public void OnExit(BallController ball) { }
}