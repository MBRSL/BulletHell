using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    #region Private properties
    private GameView _gameView;
    
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

        _Initialize();
    }

    public void Update()
    {
        _PlayerControl();
        
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
        _gameView.Initialize();

        _frameCounter = 0;
        _playerLifes = 1;
        _bullets = new List<Collidable>();
        _extends = new List<Collidable>();
    }

    private void _PlayerControl()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        _gameView.PlayerPosition += new Vector3(horizontalInput, verticalInput, 0) * Time.deltaTime * 10;
    }

    private void _CheckPlayerLifes(Collidable bullet)
    {
        _playerLifes--;
        if (_playerLifes == 0)
        {
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