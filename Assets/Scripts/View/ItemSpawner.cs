using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    #region Editor data
    [SerializeField] private GameObject _normalBulletPrefab;
    [SerializeField] private GameObjectPool _normalBulletGameObjectPool;
    [SerializeField] private GameObject _fastBulletPrefab;
    [SerializeField] private GameObjectPool _fastBulletGameObjectPool;
    [SerializeField] private GameObject _tracingBulletPrefab;
    [SerializeField] private GameObjectPool _tracingBulletGameObjectPool;
    // For "1-UP"
    [SerializeField] private GameObject _oneUpPrefab;
    [SerializeField] private GameObjectPool _oneUpGameObjectPool;
    [SerializeField] private int _initPoolGameObjectNum;

    #endregion

    #region Public methods
    public void Initialize()
    {
        _normalBulletGameObjectPool.Initialize(_normalBulletPrefab, _initPoolGameObjectNum);
        _fastBulletGameObjectPool.Initialize(_fastBulletPrefab, _initPoolGameObjectNum);
        _tracingBulletGameObjectPool.Initialize(_tracingBulletPrefab, _initPoolGameObjectNum);
        _oneUpGameObjectPool.Initialize(_oneUpPrefab, _initPoolGameObjectNum);
    }

    public Item SpawnItem(float speed, Item.Types type, Vector3 initPosition, Vector3 targetPosition)
    {
        GameObject itemGo = null;
        if (type == Item.Types.TracingBullet)
        {
            itemGo = _tracingBulletGameObjectPool.GetObjectFromPool();
        }
        else if (type == Item.Types.FastBullet)
        {
            itemGo = _fastBulletGameObjectPool.GetObjectFromPool();
            speed *= 3;
        }
        else if (type == Item.Types.NormalBullet)
        {
            itemGo = _normalBulletGameObjectPool.GetObjectFromPool();
        }
        else if (type == Item.Types.OneUp)
        {
            itemGo = _oneUpGameObjectPool.GetObjectFromPool();
        }
        itemGo.SetActive(true);
        itemGo.transform.localPosition = initPosition;
        itemGo.transform.transform.rotation = Quaternion.identity;

        var itemView = itemGo.GetComponent<ItemView>();
        itemView.Rigidbody.velocity = -(initPosition-targetPosition).normalized * speed;

        var item = new Item(type, itemView);
        return item;
    }

    public void RecycleItem(Item item)
    {
        GameObjectPool pool = null;
        if (item.Type == Item.Types.NormalBullet)
        {
            pool = _normalBulletGameObjectPool;
        }
        else if (item.Type == Item.Types.FastBullet)
        {
            pool = _fastBulletGameObjectPool;
        }
        else if (item.Type == Item.Types.TracingBullet)
        {
            pool = _tracingBulletGameObjectPool;
        }
        else if (item.Type == Item.Types.OneUp)
        {
            pool = _oneUpGameObjectPool;
        }
        pool?.ReturnObjectToPool(item.View.gameObject);
    }
    #endregion
}
