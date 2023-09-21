using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    #region Public const
    // Probability of spawning bullets is increased with frame count.
    // It spawns bullet every frame when reaching this number
    public const int MAX_BULLET_FRAME = 10000; 
    #endregion
    #region Private properties
    private GameView _gameView;
    private AiPlayer _aiPlayer;
    
    private bool _isIntroAnimationEnd;
    private bool _isGameOver;
    private int _frameCounter;
    private int _playerLifes;
    private List<Item> _items;

    #endregion

    #region Public methods
    public GameManager(GameView gameView)
    {
        _gameView = gameView;
        _gameView.OnItemHit += _UpdatePlayerLifes;
        _gameView.OnItemOutOfBounds += _RemoveItem;
        _gameView.OnPlayerOutOfBounds += _GameOver;
        _gameView.OnClickRetry += _Initialize;
        _gameView.OnIntroAnimationEnd += _IntroAnimationEnded;

        _Initialize();
    }

    public void Update()
    {
        if (!_isIntroAnimationEnd || _isGameOver)
        {
            return;
        }
        
        _frameCounter++;
        var score = _CalculateScore(_frameCounter);
        _gameView.UpdateInfo(_frameCounter, score, _playerLifes);
        _UpdateItems(_frameCounter);
        _PlayerControl();
        _AiControl();       
    }
    #endregion

    #region Private methods
    private void _Initialize()
    {
        _isIntroAnimationEnd = false;
        _isGameOver = false;
        _frameCounter = 0;
        _playerLifes = 5;
        if (_items == null)
        {
            _items = new List<Item>();
        }
        for (int i = _items.Count-1; i >= 0; i--)
        {
            _gameView.RecycleItem(_items[i]);
            _items.RemoveAt(i);
        }

        _gameView.Initialize(_playerLifes);
            _aiPlayer = new AiPlayer(
            Vector2.zero,
            _gameView.PlayerTransform,
            _gameView.PlayerBounds,
            _gameView.BorderBounds,
            _items
        );
    }

    private void _IntroAnimationEnded()
    {
        _isIntroAnimationEnd = true;
    }

    private int _CalculateScore(int frameCount)
    {
        return (int)Mathf.Pow(frameCount, 1.05f);
    }

    private void _UpdateItems(int frameCount)
    {
        float probability = Mathf.Pow((float)frameCount/MAX_BULLET_FRAME, 0.8f);
        if (Random.Range(0f, 1f) < probability)
        {
            float type = Random.Range(0f, 1f);
            if (type < 0.1f)
            {
                _items.Add(_gameView.SpawnItem(Item.Types.TracingBullet));
            }
            else if (type < 0.3f)
            {
                _items.Add(_gameView.SpawnItem(Item.Types.FastBullet));
            }
            else if (type < 0.95f)
            {
                _items.Add(_gameView.SpawnItem(Item.Types.NormalBullet));
            }
            else
            {
                _items.Add(_gameView.SpawnItem(Item.Types.OneUp));
            }
        }
        _gameView.UpdateTracingBullet(_items);
    }

    private void _PlayerControl()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        _gameView.PlayerTransform.position += new Vector3(horizontalInput, verticalInput, 0).normalized * Time.deltaTime * 10;
    }

    private void _AiControl()
    {
        var offset = _aiPlayer.GetAction().normalized;
        _gameView.PlayerTransform.position += offset * Time.deltaTime * 10;
    }

    private void _GameOver()
    {
        if (!_isIntroAnimationEnd)
        {
            return;
        }

        _isGameOver = true;
        _gameView.ShowGameOver();
    }

    private void _UpdatePlayerLifes(Item item)
    {
        if (item.Type == Item.Types.NormalBullet ||
            item.Type == Item.Types.FastBullet ||
            item.Type == Item.Types.TracingBullet)
        {
            _playerLifes--;            
        }
        else if (item.Type == Item.Types.OneUp)
        {
            _playerLifes++;
        }
        _gameView.SetPlayerLifes(_playerLifes);
        _RemoveItem(item);

        if (_playerLifes <= 0)
        {
            _GameOver();
            return;
        }
    }

    private void _RemoveItem(Item item)
    {
        _gameView.RecycleItem(item);
        _items.Remove(item);
    }
    #endregion
}
