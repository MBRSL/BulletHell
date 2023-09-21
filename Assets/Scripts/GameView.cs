using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameView : MonoBehaviour
{
    #region Public delegates
    public event Action<Item> OnItemHit;
    public event Action<Item> OnItemOutOfBounds;
    public event Action OnPlayerOutOfBounds;
    public event Action OnClickRetry;
    public event Action OnIntroAnimationEnd;
    #endregion

    #region Editor data
    [SerializeField] private PlayableDirector _introDirector;
    [SerializeField] private PlayableDirector _hitFxDirector;
    [SerializeField] private ItemView _player;
    [SerializeField] private GameObject _normalBulletPrefab;
    [SerializeField] private GameObjectPool _normalBulletGameObjectPool;
    [SerializeField] private GameObject _fastBulletPrefab;
    [SerializeField] private GameObjectPool _fastBulletGameObjectPool;
    [SerializeField] private GameObject _tracingBulletPrefab;
    [SerializeField] private GameObjectPool _tracingBulletGameObjectPool;
    // For "1-UP"
    [SerializeField] private GameObject _oneUpPrefab;
    [SerializeField] private GameObjectPool _oneUpGameObjectPool;
    [SerializeField] private SphereCollider _collidableSpace;
    [SerializeField] private BoxCollider _playerSpace;
    [SerializeField] private ParticleSystem _explosionFx;
    [SerializeField] private TextMeshProUGUI _playerLifesText;
    [SerializeField] private TextMeshProUGUI _frameCountText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _gameOverUi;
    [SerializeField] private Button _retryButton;
    [SerializeField] private float _itemSpeed;
    [SerializeField] private int _initPoolGameObjectNum;
    #endregion

    private Collider _playerCollider;
    
    #region Public functions
    public Transform PlayerTransform
    {
        get { return _player.transform; }
    }

    public Bounds PlayerBounds
    {
        get { return _player.Bounds; }
    }

    public Bounds BorderBounds
    {
        get { return _playerSpace.bounds; }
    }

    public void Initialize(int playerLifes)
    {
        _introDirector.Play();
        _retryButton.onClick.RemoveAllListeners();
        _retryButton.onClick.AddListener(_OnclickRetry);
        _gameOverUi.SetActive(false);

        _player.gameObject.SetActive(true);
        _player.transform.position = Vector3.zero;
        _player.OnLeave -= _ChechPlayerInBounds;
        _player.OnLeave += _ChechPlayerInBounds;
        _playerCollider = _player.GetComponent<Collider>();
        _normalBulletGameObjectPool.Initialize(_normalBulletPrefab, _initPoolGameObjectNum);
        _fastBulletGameObjectPool.Initialize(_fastBulletPrefab, _initPoolGameObjectNum);
        _tracingBulletGameObjectPool.Initialize(_tracingBulletPrefab, _initPoolGameObjectNum);
        _oneUpGameObjectPool.Initialize(_oneUpPrefab, _initPoolGameObjectNum);

        SetPlayerLifes(playerLifes);
    }

    public void TriggerIntroAnimationEnd()
    {
        OnIntroAnimationEnd?.Invoke();
    }

    public void ShowGameOver()
    {
        _explosionFx.transform.position = _player.transform.position;
        _explosionFx.Play();
        _player.gameObject.SetActive(false);
        _gameOverUi.SetActive(true);
    }

    public void SetPlayerLifes(int playerLifes)
    {
        _playerLifesText.text = $"Stock: {playerLifes}";
    }

    public void UpdateInfo(int frameCount, int score, int playerLifes)
    {
        _frameCountText.text = $"Frame: {frameCount}";
        _scoreText.text = $"Score: {score}";
        _playerLifesText.text = $"Lifes: {playerLifes}";
    }

    public void UpdateTracingBullet(List<Item> items)
    {
        foreach (var item in items)
        {
            if (item.Type == Item.Types.TracingBullet)
            {
                var rigidBody = item.View.Rigidbody;
                var originalMagnitude = rigidBody.velocity.magnitude;
                var toPlayerVec = _player.transform.position - item.View.transform.position;
                rigidBody.velocity = (rigidBody.velocity + toPlayerVec.normalized * 0.05f).normalized * originalMagnitude;
            }
        }
    }

    public Item SpawnItem(Item.Types type)
    {
        GameObject itemGo = null;
        Vector3 targetPosition = Vector3.zero;
        float speed = _itemSpeed;
        if (type == Item.Types.NormalBullet)
        {
            itemGo = _normalBulletGameObjectPool.GetObjectFromPool();
            targetPosition =  _collidableSpace.transform.position + _collidableSpace.radius*0.3f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        }
        else if (type == Item.Types.FastBullet)
        {
            itemGo = _fastBulletGameObjectPool.GetObjectFromPool();
            targetPosition =  _collidableSpace.transform.position + _collidableSpace.radius*0.3f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
            speed *= 3;
        }
        else if (type == Item.Types.TracingBullet)
        {
            itemGo = _tracingBulletGameObjectPool.GetObjectFromPool();
            targetPosition = _player.transform.position;
        }
        else if (type == Item.Types.OneUp)
        {
            itemGo = _oneUpGameObjectPool.GetObjectFromPool();
            targetPosition =  _collidableSpace.transform.position + _collidableSpace.radius*0.3f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        }

        var randomPointOnCircle = _SampleOnCircle(_collidableSpace);
        itemGo.SetActive(true);
        itemGo.transform.position = randomPointOnCircle;
        itemGo.transform.transform.rotation = Quaternion.identity;

        var itemView = itemGo.GetComponent<ItemView>();
        itemView.Initialize();
        itemView.Rigidbody.velocity = -(randomPointOnCircle-targetPosition).normalized * speed;

        var item = new Item(type, itemView);
        _SetEvents(item);
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

    #region Private functions
    private Vector3 _SampleOnCircle(SphereCollider collider)
    {
        var theta = Random.Range(0f, Mathf.PI);
        return collider.center + collider.radius * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
    }

    private void _SetEvents(Item item)
    {
        ItemView.HitEvent onHit = null, onLeave = null;
        onHit = (_item, _other) => 
        {
            if (_other == _playerCollider)
            {
                _item.OnHit -= onHit;
                _item.OnLeave -= onLeave;
                if (item.Type != Item.Types.OneUp)
                {
                    _hitFxDirector.Play();
                }
                OnItemHit?.Invoke(item);
            }
        };
        onLeave = (_item, _other) => 
        {
            if (_other == _collidableSpace)
            {
                _item.OnHit -= onHit;
                _item.OnLeave -= onLeave;
                OnItemOutOfBounds?.Invoke(item);
            }
        };
        item.View.OnHit += onHit;
        item.View.OnLeave += onLeave;
    }

    private void _ChechPlayerInBounds(ItemView player, Collider collider)
    {
        if (collider == _playerSpace)
        {
            OnPlayerOutOfBounds?.Invoke();
        }
    }

    private void _OnclickRetry()
    {
        OnClickRetry?.Invoke();
    }
    #endregion
}
