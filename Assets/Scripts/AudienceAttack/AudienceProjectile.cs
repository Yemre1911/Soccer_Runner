/*using UnityEngine;

[RequireComponent(typeof(PooledItem))]
public class AudienceProjectile : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform visual;
    [SerializeField] private Collider hitCollider;
    [SerializeField] private TrailRenderer trail;

    [Header("Spin (deg/sec)")]
    [SerializeField] private Vector3 spinDegPerSec = new Vector3(0f, 720f, 360f);

    [Header("Landing")]
    [SerializeField] private float landingLocalYOffset = 0f;

    private Vector3 _startLocal;
    private Vector3 _controlLocal;
    private Vector3 _endLocal;

    private Quaternion _endRot;

    private float _t;
    private float _dur;
    private bool _flying;

    private void Reset()
    {
        if (!visual && transform.childCount > 0) visual = transform.GetChild(0);
        if (!hitCollider) hitCollider = GetComponentInChildren<Collider>();
        if (!trail) trail = GetComponentInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        _flying = false;
        _t = 0f;

        if (visual) visual.localPosition = Vector3.zero;

        if (hitCollider) hitCollider.enabled = false;

        if (trail)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    public void BeginThrowWorld(Vector3 startWorld, Vector3 endWorld, float duration, float arcHeight)
    {
        if (!visual) return;

        _dur = Mathf.Max(0.05f, duration);
        _t = 0f;
        _flying = true;

        _startLocal = transform.InverseTransformPoint(startWorld);
        _endLocal = transform.InverseTransformPoint(endWorld);
        _endLocal.y += landingLocalYOffset;

        // ✅ KRİTİK FIX: Z curve'den çıkar
        _startLocal.z = 0f;
        _endLocal.z = 0f;

        Vector3 mid = (_startLocal + _endLocal) * 0.5f;
        _controlLocal = mid + Vector3.up * arcHeight;

        _endRot = visual.localRotation * Quaternion.Euler(
            Random.Range(-25f, 25f),
            Random.Range(0f, 360f),
            Random.Range(-25f, 25f)
        );

        if (hitCollider) hitCollider.enabled = false;
        if (trail) trail.emitting = true;

        visual.localPosition = _startLocal;
    }

    private void Update()
    {
        if (!_flying || !visual) return;

        _t += Time.deltaTime / _dur;
        float t = Mathf.Clamp01(_t);

        Vector3 a = Vector3.Lerp(_startLocal, _controlLocal, t);
        Vector3 b = Vector3.Lerp(_controlLocal, _endLocal, t);
        Vector3 p = Vector3.Lerp(a, b, t);

        visual.localPosition = p;
        visual.localRotation *= Quaternion.Euler(spinDegPerSec * Time.deltaTime);

        if (t >= 1f)
        {
            _flying = false;

            visual.localPosition = _endLocal;
            visual.localRotation = _endRot;

            if (hitCollider) hitCollider.enabled = true;
            if (trail) trail.emitting = false;
        }
    }
}
*/



using UnityEngine;

[RequireComponent(typeof(PooledItem))]
public class AudienceProjectile : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform visual;
    [SerializeField] private Collider hitCollider;
    [SerializeField] private TrailRenderer trail;

    [Header("Spin While Flying (deg/sec)")]
    [SerializeField] private Vector3 spinDegPerSec = new Vector3(0f, 720f, 360f);

    [Header("Flight")]
    [SerializeField] private float landingLocalYOffset = 0f;

    [Header("Settle")]
    [SerializeField] private float settleDuration = 0.18f;
    [SerializeField] private float bounceHeight = 0.08f;

    [Header("Final Rest Rotation")]
    [SerializeField] private float extraTwistMin = -20f;
    [SerializeField] private float extraTwistMax = 20f;

    private Vector3 _startLocal;
    private Vector3 _controlLocal;
    private Vector3 _endLocal;

    private float _t;
    private float _dur;
    private bool _flying;

    private bool _settling;
    private float _settleT;
    private Vector3 _impactPos;
    private Vector3 _finalPos;
    private Quaternion _impactRot;
    private Quaternion _finalRot;

    private void Reset()
    {
        if (!visual && transform.childCount > 0) visual = transform.GetChild(0);
        if (!hitCollider) hitCollider = GetComponentInChildren<Collider>(true);
        if (!trail) trail = GetComponentInChildren<TrailRenderer>(true);
    }

    private void OnEnable()
    {
        _flying = false;
        _settling = false;
        _t = 0f;
        _settleT = 0f;

        if (visual)
        {
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
        }

        if (hitCollider) hitCollider.enabled = false;

        if (trail)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    public void BeginThrowWorld(Vector3 startWorld, Vector3 endWorld, float duration, float arcHeight)
    {
        if (!visual) return;

        _dur = Mathf.Max(0.05f, duration);
        _t = 0f;
        _flying = true;
        _settling = false;

        _startLocal = transform.InverseTransformPoint(startWorld);
        _endLocal = transform.InverseTransformPoint(endWorld);

        // Bu sistemde projectile root forward taşımıyor; z'yi curve'den çıkar.
        _startLocal.z = 0f;
        _endLocal.z = 0f;

        Vector3 mid = (_startLocal + _endLocal) * 0.5f;
        _controlLocal = mid + Vector3.up * arcHeight;

        if (hitCollider) hitCollider.enabled = false;
        if (trail)
        {
            trail.Clear();
            trail.emitting = true;
        }

        visual.localPosition = _startLocal;
        visual.localRotation = Random.rotation;
    }

    private void Update()
    {
        if (!visual) return;

        if (_flying)
        {
            UpdateFlight();
            return;
        }

        if (_settling)
        {
            UpdateSettle();
        }
    }

    private void UpdateFlight()
    {
        _t += Time.deltaTime / _dur;
        float t = Mathf.Clamp01(_t);

        Vector3 a = Vector3.Lerp(_startLocal, _controlLocal, t);
        Vector3 b = Vector3.Lerp(_controlLocal, _endLocal, t);
        Vector3 p = Vector3.Lerp(a, b, t);

        visual.localPosition = p;
        visual.localRotation *= Quaternion.Euler(spinDegPerSec * Time.deltaTime);

        if (t >= 1f)
        {
            _flying = false;
            StartSettle();
        }
    }

    private void StartSettle()
    {
        _settling = true;
        _settleT = 0f;

        _impactRot = visual.localRotation;
        _finalRot = BuildNaturalRestRotation();

        float impactBottomOffset = GetBottomOffsetLocal(_impactRot);
        float finalBottomOffset = GetBottomOffsetLocal(_finalRot);

        // endLocal = zemine değmesi gereken nokta
        _impactPos = _endLocal + Vector3.up * (impactBottomOffset + landingLocalYOffset);
        _finalPos = _endLocal + Vector3.up * (finalBottomOffset + landingLocalYOffset);

        visual.localPosition = _impactPos;
    }

    private void UpdateSettle()
    {
        _settleT += Time.deltaTime / Mathf.Max(0.01f, settleDuration);
        float s = Mathf.Clamp01(_settleT);

        visual.localRotation = Quaternion.Slerp(_impactRot, _finalRot, s);

        Vector3 basePos = Vector3.Lerp(_impactPos, _finalPos, s);

        // Tek hafif sekme
        float bounce = Mathf.Sin(s * Mathf.PI) * (1f - s) * bounceHeight;
        visual.localPosition = basePos + Vector3.up * bounce;

        if (s >= 1f)
        {
            _settling = false;
            visual.localPosition = _finalPos;
            visual.localRotation = _finalRot;

            if (hitCollider) hitCollider.enabled = true;
            if (trail) trail.emitting = false;
        }
    }

    private Quaternion BuildNaturalRestRotation()
    {
        // Uzun ekseni zemine paralel olacak şekilde yatır.
        Vector3 longAxisInVisual = GetLongAxisInVisualSpace();
        if (longAxisInVisual.sqrMagnitude < 0.0001f)
            longAxisInVisual = Vector3.right;

        longAxisInVisual.Normalize();

        Vector2 planar = Random.insideUnitCircle.normalized;
        if (planar.sqrMagnitude < 0.0001f)
            planar = Vector2.right;

        Vector3 targetGroundDir = new Vector3(planar.x, 0f, planar.y).normalized;

        Quaternion layFlat = Quaternion.FromToRotation(longAxisInVisual, targetGroundDir);

        // Biraz kendi ekseni etrafında varyasyon
        float twist = Random.Range(extraTwistMin, extraTwistMax);
        layFlat = Quaternion.AngleAxis(twist, targetGroundDir) * layFlat;

        return layFlat;
    }

    private Vector3 GetLongAxisInVisualSpace()
    {
        if (!hitCollider || !visual) return Vector3.right;

        Vector3 axisLocalToCollider = Vector3.right;

        if (hitCollider is CapsuleCollider capsule)
        {
            axisLocalToCollider = capsule.direction switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                2 => Vector3.forward,
                _ => Vector3.up
            };
        }
        else if (hitCollider is BoxCollider box)
        {
            Vector3 s = box.size;
            if (s.x >= s.y && s.x >= s.z) axisLocalToCollider = Vector3.right;
            else if (s.y >= s.x && s.y >= s.z) axisLocalToCollider = Vector3.up;
            else axisLocalToCollider = Vector3.forward;
        }
        else
        {
            axisLocalToCollider = Vector3.right;
        }

        Vector3 worldAxis = hitCollider.transform.TransformDirection(axisLocalToCollider);
        Vector3 visualAxis = visual.InverseTransformDirection(worldAxis);
        return visualAxis.normalized;
    }

    private float GetBottomOffsetLocal(Quaternion visualLocalRotation)
    {
        Vector3[] pts = GetColliderSamplePointsInVisualSpace();
        if (pts == null || pts.Length == 0)
            return 0f;

        float minY = float.PositiveInfinity;

        for (int i = 0; i < pts.Length; i++)
        {
            Vector3 rotated = visualLocalRotation * pts[i];
            if (rotated.y < minY) minY = rotated.y;
        }

        return -minY;
    }

    private Vector3[] GetColliderSamplePointsInVisualSpace()
    {
        if (!hitCollider || !visual) return null;

        if (hitCollider is BoxCollider box)
            return GetBoxPointsInVisualSpace(box);

        if (hitCollider is SphereCollider sphere)
            return GetSpherePointsInVisualSpace(sphere);

        if (hitCollider is CapsuleCollider capsule)
            return GetCapsulePointsInVisualSpace(capsule);

        // Fallback
        return new[]
        {
            visual.InverseTransformPoint(hitCollider.bounds.center + Vector3.down * hitCollider.bounds.extents.y),
            visual.InverseTransformPoint(hitCollider.bounds.center + Vector3.up * hitCollider.bounds.extents.y),
        };
    }

    private Vector3[] GetBoxPointsInVisualSpace(BoxCollider box)
    {
        Vector3 c = box.center;
        Vector3 e = box.size * 0.5f;

        Vector3[] localPts =
        {
            c + new Vector3(-e.x, -e.y, -e.z),
            c + new Vector3(-e.x, -e.y,  e.z),
            c + new Vector3( e.x, -e.y, -e.z),
            c + new Vector3( e.x, -e.y,  e.z),
            c + new Vector3(-e.x,  e.y, -e.z),
            c + new Vector3(-e.x,  e.y,  e.z),
            c + new Vector3( e.x,  e.y, -e.z),
            c + new Vector3( e.x,  e.y,  e.z),
        };

        return ToVisualSpace(box.transform, localPts);
    }

    private Vector3[] GetSpherePointsInVisualSpace(SphereCollider sphere)
    {
        Vector3 c = sphere.center;
        float r = sphere.radius;

        Vector3[] localPts =
        {
            c + Vector3.down * r,
            c + Vector3.up * r,
            c + Vector3.left * r,
            c + Vector3.right * r,
            c + Vector3.forward * r,
            c + Vector3.back * r
        };

        return ToVisualSpace(sphere.transform, localPts);
    }

    private Vector3[] GetCapsulePointsInVisualSpace(CapsuleCollider capsule)
    {
        Vector3 c = capsule.center;
        float r = capsule.radius;
        float half = Mathf.Max(0f, capsule.height * 0.5f - r);

        Vector3 axis = capsule.direction switch
        {
            0 => Vector3.right,
            1 => Vector3.up,
            2 => Vector3.forward,
            _ => Vector3.up
        };

        Vector3 p1 = c + axis * half;
        Vector3 p2 = c - axis * half;

        Vector3 sideA, sideB;
        if (Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > 0.9f)
        {
            sideA = Vector3.right;
            sideB = Vector3.forward;
        }
        else
        {
            sideA = Vector3.up;
            sideB = Vector3.Cross(axis, sideA).normalized;
        }

        Vector3[] localPts =
        {
            p1 + axis * r,
            p2 - axis * r,
            p1 + sideA * r,
            p1 - sideA * r,
            p1 + sideB * r,
            p1 - sideB * r,
            p2 + sideA * r,
            p2 - sideA * r,
            p2 + sideB * r,
            p2 - sideB * r
        };

        return ToVisualSpace(capsule.transform, localPts);
    }

    private Vector3[] ToVisualSpace(Transform source, Vector3[] sourceLocalPoints)
    {
        Vector3[] result = new Vector3[sourceLocalPoints.Length];

        for (int i = 0; i < sourceLocalPoints.Length; i++)
        {
            Vector3 world = source.TransformPoint(sourceLocalPoints[i]);
            result[i] = visual.InverseTransformPoint(world);
        }

        return result;
    }
}




/*
using UnityEngine;

[RequireComponent(typeof(PooledItem))]
public class AudienceProjectile : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform visual;
    [SerializeField] private Collider hitCollider;
    [SerializeField] private TrailRenderer trail;

    [Header("Spin (deg/sec)")]
    [SerializeField] private Vector3 spinDegPerSec = new Vector3(0f, 720f, 360f);

    [Header("Landing Position")]
    [Tooltip("Mesh pivot zemine uymuyorsa inişte Y offset (local).")]
    [SerializeField] private float landingLocalYOffset = 0f;

    public enum LandingRotationMode
    {
        KeepEndRot,
        UprightRandomYaw,
        FlatOnGroundRandomYaw,
        CustomEuler
    }

    [Header("Landing Rotation")]
    [SerializeField] private LandingRotationMode landingRotationMode = LandingRotationMode.FlatOnGroundRandomYaw;

    [Tooltip("Flat/Custom modlarda kullanılacak base euler. (Torch için genelde X=90 veya Z=90).")]
    [SerializeField] private Vector3 landingBaseEuler = new Vector3(90f, 0f, 0f);

    [Tooltip("Yaw random aralığı (derece).")]
    [SerializeField] private Vector2 landingYawRange = new Vector2(0f, 360f);

    [Header("Rotation Smoothing")]
    [Tooltip("Uçuş boyunca landing rotasyonuna yaklaşma miktarı. 1 = tamamen landing'e oturur.")]
    [Range(0f, 1f)]
    [SerializeField] private float landAlignStrength = 0.5f;

    [Header("Collider Follow Visual")]
    [Tooltip("Collider root'taysa proxy collider üretir. Collider'ı visual ile hizalar.")]
    [SerializeField] private bool snapColliderToVisual = true;

    [Tooltip("Collider'ı visual'a hizalarken ekstra local offset.")]
    [SerializeField] private Vector3 colliderLocalOffset = Vector3.zero;

    [Tooltip("Collider rotasyonu da visual'ı takip etsin mi? (Torch yatık duracaksa TRUE iyi.)")]
    [SerializeField] private bool syncColliderRotation = true;

    [Header("Settle (optional, physics-free)")]
    [Tooltip("İnişte hafif sekme/yumuşama. 0 ise kapalı.")]
    [SerializeField] private float settleDuration = 0.18f;

    [SerializeField] private float settleBounceHeight = 0.18f;

    // Curve points (root local space)
    private Vector3 _startLocal;
    private Vector3 _controlLocal;
    private Vector3 _endLocal;

    // Rotation blending
    private Quaternion _startRot;
    private Quaternion _landRot;

    // ✅ NEW: spin birikimi (baseRot her frame yeniden kurulduğu için spin hissi kaybolmasın)
    private Quaternion _spinAccum = Quaternion.identity;

    // Timing
    private float _t;
    private float _dur;
    private bool _flying;

    // Settle
    private bool _settling;
    private float _settleT;

    // collider carrier (root collider ise proxy oluşturur)
    private Transform _colliderCarrier;

    private void Reset()
    {
        if (!visual && transform.childCount > 0) visual = transform.GetChild(0);
        if (!hitCollider) hitCollider = GetComponentInChildren<Collider>();
        if (!trail) trail = GetComponentInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        _flying = false;
        _settling = false;
        _t = 0f;
        _settleT = 0f;

        _spinAccum = Quaternion.identity;

        EnsureColliderCarrier();

        if (visual) visual.localPosition = Vector3.zero;

        if (hitCollider) hitCollider.enabled = false;

        if (trail)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    /// <summary>
    /// Start/End WORLD verilir. Script bunları root'a göre local offset'e çevirip curve uygular.
    /// Z curve dışı: Z'yi WorldScrollMover taşıyor.
    /// </summary>
    public void BeginThrowWorld(Vector3 startWorld, Vector3 endWorld, float duration, float arcHeight)
    {
        if (!visual) return;

        EnsureColliderCarrier();

        _dur = Mathf.Max(0.05f, duration);
        _t = 0f;
        _flying = true;
        _settling = false;
        _settleT = 0f;

        _spinAccum = Quaternion.identity;

        // World -> local (root space)
        _startLocal = transform.InverseTransformPoint(startWorld);
        _endLocal = transform.InverseTransformPoint(endWorld);
        _endLocal.y += landingLocalYOffset;

        // ✅ Z curve'den çıkar
        _startLocal.z = 0f;
        _endLocal.z = 0f;

        Vector3 mid = (_startLocal + _endLocal) * 0.5f;
        _controlLocal = mid + Vector3.up * arcHeight;

        // Rotation plan
        _startRot = visual.localRotation;
        _landRot = ComputeLandingRotation();

        // Reset visuals
        if (hitCollider) hitCollider.enabled = false;
        if (trail) trail.emitting = true;

        visual.localPosition = _startLocal;

        if (snapColliderToVisual && hitCollider)
            SnapColliderToVisual();
    }

    private void Update()
    {
        if (_flying)
            TickFlying();
        else if (_settling)
            TickSettle();
    }

    private void TickFlying()
    {
        if (!visual) return;

        _t += Time.deltaTime / _dur;
        float t = Mathf.Clamp01(_t);

        // Bezier
        Vector3 a = Vector3.Lerp(_startLocal, _controlLocal, t);
        Vector3 b = Vector3.Lerp(_controlLocal, _endLocal, t);
        Vector3 p = Vector3.Lerp(a, b, t);
        visual.localPosition = p;

        // ✅ Snap yok: uçuş boyunca landing rot'a yaklaş
        float alignT = Mathf.Clamp01(t * Mathf.Max(0.01f, landAlignStrength));
        Quaternion baseRot = Quaternion.Slerp(_startRot, _landRot, alignT);

        // ✅ Spin birikimi: base rot her frame yenilense bile spin hissi korunur
        _spinAccum = _spinAccum * Quaternion.Euler(spinDegPerSec * Time.deltaTime);
        visual.localRotation = baseRot * _spinAccum;

        if (snapColliderToVisual && hitCollider)
            SnapColliderToVisual();

        if (t >= 1f)
        {
            _flying = false;

            visual.localPosition = _endLocal;
            visual.localRotation = _landRot; // landing'de artık sabitle (spawn hissi yok)

            if (snapColliderToVisual && hitCollider)
                SnapColliderToVisual();

            if (hitCollider) hitCollider.enabled = true;
            if (trail) trail.emitting = false;

            if (settleDuration > 0.001f && settleBounceHeight > 0.001f)
            {
                _settling = true;
                _settleT = 0f;
            }
        }
    }

    private void TickSettle()
    {
        if (!visual) { _settling = false; return; }

        _settleT += Time.deltaTime / Mathf.Max(0.01f, settleDuration);
        float t = Mathf.Clamp01(_settleT);

        // Damped bounce
        float bounce = Mathf.Sin(t * Mathf.PI) * (1f - t) * settleBounceHeight;

        visual.localPosition = new Vector3(_endLocal.x, _endLocal.y + bounce, _endLocal.z);
        visual.localRotation = _landRot;

        if (snapColliderToVisual && hitCollider && hitCollider.enabled)
            SnapColliderToVisual();

        if (t >= 1f)
        {
            visual.localPosition = _endLocal;
            visual.localRotation = _landRot;

            if (snapColliderToVisual && hitCollider && hitCollider.enabled)
                SnapColliderToVisual();

            _settling = false;
        }
    }

    private Quaternion ComputeLandingRotation()
    {
        float yaw = Random.Range(landingYawRange.x, landingYawRange.y);

        switch (landingRotationMode)
        {
            case LandingRotationMode.UprightRandomYaw:
                return Quaternion.Euler(0f, yaw, 0f);

            case LandingRotationMode.FlatOnGroundRandomYaw:
                return Quaternion.Euler(landingBaseEuler.x, yaw + landingBaseEuler.y, landingBaseEuler.z);

            case LandingRotationMode.CustomEuler:
                return Quaternion.Euler(landingBaseEuler.x, yaw + landingBaseEuler.y, landingBaseEuler.z);

            default:
                // KeepEndRot: kaotik
                return visual.localRotation * Quaternion.Euler(
                    Random.Range(-25f, 25f),
                    Random.Range(0f, 360f),
                    Random.Range(-25f, 25f)
                );
        }
    }

    // -------------------------
    // Collider proxy & syncing
    // -------------------------
    private void EnsureColliderCarrier()
    {
        if (!hitCollider) return;

        // Collider root'ta değilse: carrier collider transform
        if (hitCollider.transform != transform)
        {
            _colliderCarrier = hitCollider.transform;
            return;
        }

        // Collider root'ta -> proxy oluştur/kullan
        var existing = transform.Find("__ColliderProxy");
        if (!existing)
        {
            var go = new GameObject("__ColliderProxy");
            go.transform.SetParent(transform, false);
            existing = go.transform;
        }

        // Proxy'de collider yoksa, root collider'ı kopyala
        var proxyCol = existing.GetComponent<Collider>();
        if (!proxyCol)
            proxyCol = CopyCollider(hitCollider, existing.gameObject);

        // Root collider'ı kapat, proxy collider'ı kullan
        hitCollider.enabled = false;
        hitCollider = proxyCol;

        _colliderCarrier = hitCollider.transform;
    }

    private Collider CopyCollider(Collider src, GameObject dst)
    {
        if (src is BoxCollider b)
        {
            var c = dst.AddComponent<BoxCollider>();
            c.center = b.center;
            c.size = b.size;
            c.isTrigger = b.isTrigger;
            return c;
        }

        if (src is SphereCollider s)
        {
            var c = dst.AddComponent<SphereCollider>();
            c.center = s.center;
            c.radius = s.radius;
            c.isTrigger = s.isTrigger;
            return c;
        }

        if (src is CapsuleCollider cap)
        {
            var c = dst.AddComponent<CapsuleCollider>();
            c.center = cap.center;
            c.radius = cap.radius;
            c.height = cap.height;
            c.direction = cap.direction;
            c.isTrigger = cap.isTrigger;
            return c;
        }

        if (src is MeshCollider m)
        {
            var c = dst.AddComponent<MeshCollider>();
            c.sharedMesh = m.sharedMesh;
            c.convex = m.convex;
            c.isTrigger = m.isTrigger;
            return c;
        }

        var fallback = dst.AddComponent<BoxCollider>();
        fallback.isTrigger = true;
        return fallback;
    }

    private void SnapColliderToVisual()
    {
        EnsureColliderCarrier();
        if (!_colliderCarrier || !visual) return;

        _colliderCarrier.localPosition = visual.localPosition + colliderLocalOffset;

        if (syncColliderRotation)
            _colliderCarrier.localRotation = visual.localRotation;
    }
}*/
