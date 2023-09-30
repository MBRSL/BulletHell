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
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private SphereCollider _itemSpawnSpace;
    [SerializeField] private ItemView _player;
    [SerializeField] private BoxCollider _playerSpace;
    [SerializeField] private ParticleSystem _explosionFx;
    [SerializeField] private TextMeshProUGUI _infoText;
    [SerializeField] private TextMeshProUGUI _playerLifesText;
    [SerializeField] private GameObject _gameOverUi;
    [SerializeField] private Button _retryButton;
    [SerializeField] private float _playerSpeed;
    [SerializeField] private float _itemSpeed;
    #endregion

    #region Private properties
    private Collider _playerCollider;
    #endregion

    #region Public functions
    public float PlayerSpeed { get { return _playerSpeed; } }
    public float ItemSpeed { get { return _itemSpeed; } }
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

    public Bounds ItemSpawnBounds
    {
        get { return _itemSpawnSpace.bounds; }
    }

    public void Initialize(Vector3 playerInitPosition, int playerLifes)
    {
        //_introDirector.Play();
        _retryButton.onClick.RemoveAllListeners();
        _retryButton.onClick.AddListener(_OnclickRetry);
        _gameOverUi.SetActive(false);

        _player.gameObject.SetActive(true);
        _player.transform.localPosition = playerInitPosition;

        _itemSpawner.Initialize();
        _player.OnLeave -= _ChechPlayerInBounds;
        _player.OnLeave += _ChechPlayerInBounds;
        _playerCollider = _player.GetComponent<Collider>();

        SetPlayerLifes(playerLifes);
    }

    public void TriggerIntroAnimationEnd()
    {
        OnIntroAnimationEnd?.Invoke();
    }

    public void ShowGameOver()
    {
        _explosionFx.transform.localPosition = _player.transform.localPosition;
        _explosionFx.Play();
        _player.gameObject.SetActive(false);
        //_gameOverUi.SetActive(true);
    }

    public void SetPlayerLifes(int playerLifes)
    {
        _playerLifesText.text = $"Lifes: {playerLifes}";
    }

    public void UpdateInfo(
        int frameCount,
        int score,
        float bulletDistance,
        float oneUpDistance,
        float borderDistance,
        int hits,
        int oneUps,
        float cumulativeReward,
        string trainingMode,
        int playerLifes)
    {
        _infoText.text = "";
        _infoText.text += $"Frame: {frameCount}\n";
        _infoText.text += $"Score: {score}\n";
        _infoText.text += $"Bullet: {bulletDistance:F3}\n";
        _infoText.text += $"OneUp: {oneUpDistance:F3}\n";
        _infoText.text += $"Border: {borderDistance:F3}\n";
        _infoText.text += $"Hits: {hits}\n";
        _infoText.text += $"OneUps: {oneUps}\n";
        _infoText.text += $"Cumulative Reward: {cumulativeReward:F3}\n";
        _infoText.text += $"Training: {trainingMode}\n";
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
                var toPlayerVec = _player.transform.localPosition - item.View.transform.localPosition;
                rigidBody.velocity = (rigidBody.velocity + toPlayerVec.normalized * Time.fixedDeltaTime).normalized * originalMagnitude;
            }
        }
    }

    public Item SpawnItem(Item.Types type, Vector3 initPosition, Vector3 targetPosition)
    {
        var item = _itemSpawner.SpawnItem(_itemSpeed, type, initPosition, targetPosition);
        _SetEvents(item);
        return item;
    }

    public void RecycleItem(Item item)
    {
        _itemSpawner.RecycleItem(item);
    }

    #endregion

    #region Private functions
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
            if (_other == _itemSpawnSpace)
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
