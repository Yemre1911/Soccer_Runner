/*using UnityEngine;

public class PooledItem : MonoBehaviour
{
    private ItemPooler _pooler;
    public string PoolId { get; private set; }

    public void Bind(ItemPooler pooler, string id)
    {
        _pooler = pooler;
        PoolId = id;
    }

    public virtual void OnSpawned() { }

    public void Despawn()
    {
        if (_pooler == null || string.IsNullOrEmpty(PoolId))
        {
            gameObject.SetActive(false);
            return;
        }

        _pooler.Return(PoolId, gameObject);
    }
}*/

using UnityEngine;

public class PooledItem : MonoBehaviour
{
    private ItemPooler _pooler;
    public string PoolId { get; private set; }

    public void Bind(ItemPooler pooler, string id)
    {
        _pooler = pooler;
        PoolId = id;
    }

    public virtual void OnSpawned() { }

    // (opsiyonel) ileride lazım olur (sfx/vfx reset vb)
    public virtual void OnDespawned() { }

    public void Despawn()
    {
        if (_pooler == null || string.IsNullOrEmpty(PoolId))
        {
            gameObject.SetActive(false);
            return;
        }

        OnDespawned();
        _pooler.Return(PoolId, gameObject);
    }
}