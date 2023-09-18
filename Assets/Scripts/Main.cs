using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObjectPool _bulletGameObjectPool;
    [SerializeField] private SphereCollider _bulletActiveSpace;
    [SerializeField] private float bulletSpeed;

    private int _counter;

    void Start()
    {
        Application.targetFrameRate = 60;
        _counter = 0;
        _bulletGameObjectPool.Initiate(_bulletPrefab, 10);
    }

    void Update()
    {
        _PlayerControl();
        
        _counter++;
        if (_counter % 10 == 0)
        {
            _SpawnBullet();
            _counter = 0;
        }
    }

    private void _PlayerControl()
    {
        var horizontalInput = Input.GetAxis("Horizontal");
        var verticalInput = Input.GetAxis("Vertical");
        _player.transform.position += new Vector3(horizontalInput, verticalInput, 0) * Time.deltaTime;
    }

    private Vector3 _SampleOnCircle(SphereCollider collider)
    {
        var theta = Random.Range(0f, Mathf.PI);
        return collider.center + collider.radius * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
    }

    private void _SpawnBullet()
    {
        var randomPointOnCircle = _SampleOnCircle(_bulletActiveSpace);
        var bulletGO = _bulletGameObjectPool.GetObjectFromPool();
        bulletGO.SetActive(true);
        bulletGO.transform.position = randomPointOnCircle;
        bulletGO.transform.transform.rotation = Quaternion.identity;

        var bulletRigidbody = bulletGO.GetComponent<Rigidbody>();
        var targetPosition = randomPointOnCircle + _bulletActiveSpace.radius*0.1f*new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0);
        bulletRigidbody.velocity = -(targetPosition-_bulletActiveSpace.transform.position).normalized * bulletSpeed;

        var bullet = bulletGO.GetComponent<Bullet>();
        bullet.Init(_bulletActiveSpace);
        bullet.OnExitingSpace += _DestroyBullet;
    }

    private void _DestroyBullet(Bullet bullet)
    {
        bullet.OnExitingSpace -= _DestroyBullet;
        _bulletGameObjectPool.ReturnObjectToPool(bullet.gameObject);
    }
}
