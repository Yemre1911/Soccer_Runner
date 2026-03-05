using UnityEngine;

/// <summary>
/// DIP: IRunDistance arıyoruz. Eğer GameManager IRunDistance değilse,
/// burada adapter gibi çalışıp GameManager.Instance.RunDistance okumaya çalışır.
/// </summary>
public class RunDistanceProvider : MonoBehaviour, IRunDistance
{
    public float RunDistance
    {
        get
        {
            // 1) GameManager IRunDistance ise onu kullan
            if (GameManager.Instance is IRunDistance rd) return rd.RunDistance;

            // 2) GameManager'da RunDistance property/field varsa (senin projendeki isim)
            // Aşağıdaki satırı projendeki gerçek isimle değiştir:
            return GameManager.Instance != null ? GameManager.Instance.RunDistance : 0f;
        }
    }
}