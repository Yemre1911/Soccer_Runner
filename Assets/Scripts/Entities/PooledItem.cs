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

    public void Despawn()
    {
        if (_pooler == null || string.IsNullOrEmpty(PoolId))
        {
            gameObject.SetActive(false);
            return;
        }

        _pooler.Return(PoolId, gameObject);
    }
}