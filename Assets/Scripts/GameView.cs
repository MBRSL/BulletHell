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
    public event Action<Collidable> OnBulletHit;
    public event Action<Collidable> OnExtendHit;
    public event Action<Collidable> OnBulletOutOfBounds;
    public event Action<Collidable> OnExtendOutOfBounds;
    public event Action OnPlayerOutOfBounds;
    public event Action OnClickRetry;
    public event Action OnIntroAnimationEnd;
    #endregion

    #region Editor data
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private Collidable _player;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObjectPool _bulletGameObjectPool;
    // For "1-UP"
    [SerializeField] private GameObject _extendPrefab;
    [SerializeField] private GameObjectPool _extendGameObjectPool;
    [SerializeField] private SphereCollider _collidableSpace;
    [SerializeField] private BoxCollider _playerSpace;
    [SerializeField] private TextMeshProUGUI _playerLifesText;
    [SerializeField] private TextMeshProUGUI _frameCountText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _gameOverUi;
    [SerializeField] private Button _retryButton;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _extendSpeed;
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
        get { return _playerSpace.bounds; }
    }

    public void Initialize(int playerLifes)
    {
        _director.Play();
        _retryButton.onClick.RemoveAllListeners();
        _retryButton.onClick.AddListener(_OnclickRetry);
        _gameOverUi.SetActive(false);

        //_player.transform.position = Vector3.zero;
        _player.OnLeave -= _ChechPlayerInBounds;
        _player.OnLeave += _ChechPlayerInBounds;
        _playerCollider = _player.GetComponent<Collider>();
        _bulletGameObjectPool.Initialize(_bulletPrefab, _initPoolGameObjectNum);
        _extendGameObjectPool.Initialize(_extendPrefab, _initPoolGameObjectNum);

        SetPlayerLifes(playerLifes);
    }

    public void TriggerIntroAnimationEnd()
    {
        OnIntroAnimationEnd?.Invoke();
    }

    public void ShowGameOver()
    {
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

    public Collidable SpawnBullet()
    {
        var randomPointOnCircle = _SampleOnCircle(_collidableSpace);
        var bulletGO = _bulletGameObjectPool.GetObjectFromPool();
        bulletGO.SetActive(true);
        bulletGO.transform.position = randomPointOnCircle;
        bulletGO.transform.transform.rotation = Quaternion.identity;

        var bulletRigidbody = bulletGO.GetComponent<Rigidbody>();
        var targetPosition = randomPointOnCircle + _collidableSpace.radius*0.1f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        bulletRigidbody.velocity = -(targetPosition-_collidableSpace.transform.position).normalized * _bulletSpeed;

        var bullet = bulletGO.GetComponent<Collidable>();
        bullet.OnHit += _BulletHit;
        bullet.OnLeave += _BulletLeave;
        return bullet;
    }

    public Collidable SpawnExtends()
    {
        var randomPointOnCircle = _SampleOnCircle(_collidableSpace);
        var extendGO = _extendGameObjectPool.GetObjectFromPool();
        extendGO.SetActive(true);
        extendGO.transform.position = randomPointOnCircle;
        extendGO.transform.transform.rotation = Quaternion.identity;

        var extendRigidbody = extendGO.GetComponent<Rigidbody>();
        var targetPosition = randomPointOnCircle + _collidableSpace.radius*0.1f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        extendRigidbody.velocity = -(targetPosition-_collidableSpace.transform.position).normalized * _extendSpeed;

        var extend = extendGO.GetComponent<Collidable>();
        extend.OnHit += _ExtendHit;
        extend.OnLeave += _ExtendLeave;
        return extend;
    }

    public void RecycleBullet(Collidable bullet)
    {
        _bulletGameObjectPool.ReturnObjectToPool(bullet.gameObject);
    }

    public void RecycleExtend(Collidable extend)
    {
        _extendGameObjectPool.ReturnObjectToPool(extend.gameObject);
    }
    #endregion

    #region Private functions
    private Vector3 _SampleOnCircle(SphereCollider collider)
    {
        var theta = Random.Range(0f, Mathf.PI);
        return collider.center + collider.radius * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
    }

    private void _ChechPlayerInBounds(Collidable player, Collider collider)
    {
        if (collider == _playerSpace)
        {
            OnPlayerOutOfBounds?.Invoke();
        }
    }

    private void _BulletHit(Collidable bullet, Collider collider)
    {
        if (collider == _playerCollider)
        {
            Debug.Log("Bullet Hit");
            OnBulletHit?.Invoke(bullet);
        }
    }

    private void _BulletLeave(Collidable bullet, Collider collider)
    {
        if (collider == _collidableSpace)
        {
            OnBulletOutOfBounds?.Invoke(bullet);
        }
    }

    private void _ExtendHit(Collidable extend, Collider collider)
    {
        if (collider == _playerCollider)
        {
            OnExtendHit?.Invoke(extend);
        }
    }

    private void _ExtendLeave(Collidable extend, Collider collider)
    {
        if (collider == _collidableSpace)
        {
            OnExtendOutOfBounds?.Invoke(extend);
        }
    }

    private void _OnclickRetry()
    {
        OnClickRetry?.Invoke();
    }
    #endregion
}
