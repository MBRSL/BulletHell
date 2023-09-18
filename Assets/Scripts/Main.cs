using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    #region Editor data
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObjectPool _bulletGameObjectPool;
    // For "1-UP"
    [SerializeField] private GameObject _extendPrefab;
    [SerializeField] private GameObjectPool _extendGameObjectPool;
    [SerializeField] private SphereCollider _activeSpace;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float extendSpeed;
    [SerializeField] private float playerMovingSpeed;
    #endregion
    
    private int _counter;
    private Collider _playerCollider;
    private List<Collidable> _bullets;
    private List<Collidable> _extends;

    #region Unity functions
    void Start()
    {
        Application.targetFrameRate = 60;

        _counter = 0;
        _playerCollider = _player.GetComponent<Collider>();
        _bullets = new List<Collidable>();
        _bulletGameObjectPool.Initiate(_bulletPrefab, 10);

        _extends = new List<Collidable>();
        _extendGameObjectPool.Initiate(_extendPrefab, 10);
    }

    void Update()
    {
        _PlayerControl();
        
        _counter++;
        if (_counter % 10 == 0)
        {
            _SpawnBullet();
        }
        if (_counter % 100 == 0)
        {
            _SpawnExtends();
            _counter = 0;
        }
    }
    #endregion

    private void _PlayerControl()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        _player.transform.position += new Vector3(horizontalInput, verticalInput, 0) * Time.deltaTime * playerMovingSpeed;
    }

    private Vector3 _SampleOnCircle(SphereCollider collider)
    {
        var theta = Random.Range(0f, Mathf.PI);
        return collider.center + collider.radius * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
    }

    private void _SpawnBullet()
    {
        var randomPointOnCircle = _SampleOnCircle(_activeSpace);
        var bulletGO = _bulletGameObjectPool.GetObjectFromPool();
        bulletGO.SetActive(true);
        bulletGO.transform.position = randomPointOnCircle;
        bulletGO.transform.transform.rotation = Quaternion.identity;

        var bulletRigidbody = bulletGO.GetComponent<Rigidbody>();
        var targetPosition = randomPointOnCircle + _activeSpace.radius*0.1f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        bulletRigidbody.velocity = -(targetPosition-_activeSpace.transform.position).normalized * bulletSpeed;

        var bullet = bulletGO.GetComponent<Collidable>();
        _bullets.Add(bullet);
        bullet.OnHit += _CheckPlayerHit;
        bullet.OnLeave += _RecycleBullet;
    }

    private void _SpawnExtends()
    {
        var randomPointOnCircle = _SampleOnCircle(_activeSpace);
        var extendGO = _extendGameObjectPool.GetObjectFromPool();
        extendGO.SetActive(true);
        extendGO.transform.position = randomPointOnCircle;
        extendGO.transform.transform.rotation = Quaternion.identity;

        var extendRigidbody = extendGO.GetComponent<Rigidbody>();
        var targetPosition = randomPointOnCircle + _activeSpace.radius*0.1f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        extendRigidbody.velocity = -(targetPosition-_activeSpace.transform.position).normalized * extendSpeed;

        var extend = extendGO.GetComponent<Collidable>();
        _extends.Add(extend);
        extend.OnHit += _ExtendLifes;
    }

    private void _ExtendLifes(Collidable extend, Collider collider)
    {
        if (collider == _playerCollider)
        {
            extend.OnHit -= _ExtendLifes;
            _extends.Remove(extend);
            _extendGameObjectPool.ReturnObjectToPool(extend.gameObject);
        }
    }

    private void _CheckPlayerHit(Collidable bullet, Collider collider)
    {
        if (collider == _playerCollider)
        {
            Debug.Log("Hit");
        }
    }

    private void _RecycleBullet(Collidable bullet, Collider collider)
    {
        if (collider == _activeSpace)
        {
            bullet.OnHit -= _CheckPlayerHit;
            bullet.OnLeave -= _RecycleBullet;
            _bullets.Remove(bullet);
            _bulletGameObjectPool.ReturnObjectToPool(bullet.gameObject);
        }
    }
}
