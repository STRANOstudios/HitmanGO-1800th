using UnityEngine;

public class PoolInitializer : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public GameObject prefab;
        public int amount;
    }

    [Header("Pool Settings")]
    [SerializeField] private PoolItem[] itemsToPool;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        foreach (var item in itemsToPool)
        {
            if (item.prefab != null)
                ObjectPooler.Instance.InitializePool(item.prefab, item.amount);
        }
    }

    public void ClearPool()
    {
        foreach (var item in itemsToPool)
        {
            if (item.prefab != null)
                ObjectPooler.Instance.RemoveObjectFromPool(item.prefab);
        }
    }
}
