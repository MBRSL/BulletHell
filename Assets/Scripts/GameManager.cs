using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    #region Private properties
    private GameView _gameView;
    private AiPlayer _aiPlayer;
    
    private bool _isGameOver;
    private int _frameCounter;
    private int _playerLifes;
    private List<Collidable> _bullets;
    private List<Collidable> _extends;

    #endregion

    #region Public methods
    public GameManager(GameView gameView)
    {
        _gameView = gameView;
        _gameView.OnBulletHit += _CheckPlayerLifes;
        _gameView.OnBulletOutOfBounds += _RemoveBullet;
        _gameView.OnExtendHit += _AddPlayerLifes;
        _gameView.OnExtendOutOfBounds += _RemoveExtend;
        _gameView.OnPlayerOutOfBounds += _GameOver;
        _gameView.OnClickRetry += _Initialize;

        _Initialize();
    }

    public void Update()
    {
        if (_isGameOver)
        {
            return;
        }
        
        _PlayerControl();
        _AiControl();
        
        _frameCounter++;
        if (_frameCounter % 10 == 0)
        {
            var bullet = _gameView.SpawnBullet();            
            _bullets.Add(bullet);
        }
        if (_frameCounter % 100 == 0)
        {
            var extend = _gameView.SpawnExtends();
            _extends.Add(extend);

            _frameCounter = 0;
        }
    }
    #endregion

    #region Private methods
    private void _Initialize()
    {
        _isGameOver = false;
        _frameCounter = 0;
        _playerLifes = 1;
        _bullets = new List<Collidable>();
        _extends = new List<Collidable>();

        _gameView.Initialize(_playerLifes);
            _aiPlayer = new AiPlayer(
            Vector2.zero,
            _gameView.PlayerTransform,
            _gameView.PlayerBounds,
            _bullets,
            _extends
        );

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
        _isGameOver = true;
        _gameView.ShowGameOver();
    }

    private void _CheckPlayerLifes(Collidable bullet)
    {
        _playerLifes--;
        _gameView.SetPlayerLifes(_playerLifes);

        if (_playerLifes <= 0)
        {
            _GameOver();
            return;
        }

        _RemoveBullet(bullet);
    }

    private void _RemoveBullet(Collidable bullet)
    {
        _bullets.Remove(bullet);
        _gameView.RecycleBullet(bullet);
    }

    private void _AddPlayerLifes(Collidable extend)
    {
        _playerLifes++;
        _RemoveExtend(extend);
    }

    private void _RemoveExtend(Collidable extend)
    {
        _extends.Remove(extend);
        _gameView.RecycleExtend(extend);
    }
    #endregion
}
