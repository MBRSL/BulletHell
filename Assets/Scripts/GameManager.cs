using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class GameManager
{
    #region Public properties    
    public enum TrainingMode
    {
        Border,
        LessBullets,
        NormalBullets,
        IncreasingBullets
    }
    #endregion

    #region Private properties
    private GameView _gameView;
    private DodgeAgent _dodgeAgent;
    private OldDodgeAgent _oldDodgeAgent;
    
    private Trainer _trainer;
    private bool _isIntroAnimationEnd;
    private bool _isGameOver;
    private int _frameCounter;
    private int _hitCounter;
    private int _oneUpCounter;
    private int _playerLifes;
    private List<Item> _items;
    private Vector3 _currentAiMovement;
    private float _defaultTrainingMode;
    #endregion

    #region Public methods
    public GameManager(GameView gameView, DodgeAgent dodgeAgent, float defaultTrainingMode)
    {
        _gameView = gameView;
        _gameView.OnItemHit += _UpdatePlayerLifes;
        _gameView.OnItemOutOfBounds += _RemoveItem;
        _gameView.OnPlayerOutOfBounds += _OutOfBound;
        _gameView.OnClickRetry += _Initialize;
        _gameView.OnIntroAnimationEnd += _IntroAnimationEnded;

        _dodgeAgent = dodgeAgent;
        _dodgeAgent.OnMoving += _ReceiveAiMovement;
        
        _items = new List<Item>();

        _defaultTrainingMode = Academy.Instance.EnvironmentParameters.GetWithDefault("trainingMode", defaultTrainingMode);
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
        var reward = _dodgeAgent.RewardSource;
        _gameView.UpdateInfo(
            _frameCounter,
            score,
            reward.PrevBulletDistance,
            reward.PrevOneUpDistance,
            reward.PrevBorderDistance,
            _hitCounter,
            _oneUpCounter,
            _dodgeAgent.GetCumulativeReward(),
            _trainer.ToString(),
            _playerLifes);
        _gameView.UpdateRewardVisualizer(reward);
        _UpdateItems(_frameCounter);
        _PlayerControl();
        _AiControl(_currentAiMovement);

        if (_trainer.ShouldEndEpisode(_frameCounter))
        {
            _dodgeAgent.Pass();
            _dodgeAgent.EndEpisode();
            _Initialize();
        }
    }
    #endregion

    #region Private methods
    private void _Initialize()
    {
        _isIntroAnimationEnd = false;
        _isGameOver = false;
        _frameCounter = 0;
        _hitCounter = 0;
        _oneUpCounter = 0;
        for (int i = _items.Count-1; i >= 0; i--)
        {
            _gameView.RecycleItem(_items[i]);
            _items.RemoveAt(i);
        }

        _InitTrainer(_defaultTrainingMode);
        _playerLifes = _trainer.GetInitPlayerLifes();
        _gameView.Initialize(_trainer.GetInitPlayerPosition(), _playerLifes);
        _oldDodgeAgent = new OldDodgeAgent
        (
            _gameView.PlayerTransform,
            _gameView.PlayerBounds,
            _gameView.BorderBounds,
            _items
        );

        _dodgeAgent.InjectData
        (
            _oldDodgeAgent,
            _gameView.PlayerTransform,
            _gameView.BorderBounds,
            _gameView.PlayerBounds.extents.magnitude,
            _items
        );
    }

    private void _InitTrainer(float mode)
    {
        // Mix mode
        if (mode < 0)
        {
            mode = Random.Range(0, 3);
        }

        if (mode <= 0)
        {
            _trainer = new BorderTrainer(_gameView.BorderBounds);
        }
        else if (mode <= 1)
        {
            _trainer = new SingleItemTrainer(_gameView.ItemSpawnBounds);
        }
        else if (mode <= 2)
        {
            _trainer = new NormalItemTrainer(_gameView.ItemSpawnBounds, _gameView.BorderBounds);
        }
        else
        {
            _trainer = new IncreasingItemTrainer(_gameView.ItemSpawnBounds, _gameView.BorderBounds);
        }
    }

    private void _IntroAnimationEnded()
    {
        _isIntroAnimationEnd = true;
        _dodgeAgent.OnEpisodeBegin();
    }

    private int _CalculateScore(int frameCount)
    {
        return (int)Mathf.Pow(frameCount, 1.05f);
    }

    private void _UpdateItems(int frameCount)
    {
        if (_trainer.ShouldSpawnItem(frameCount))
        {
            var spawnItemData = _trainer.GetSpawnItemData();
            var item = _gameView.SpawnItem(spawnItemData.Type, spawnItemData.InitPosition, spawnItemData.TargetPosition);
            _items.Add(item);
        }
        _gameView.UpdateTracingBullet(_items);
    }

    private void _PlayerControl()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        _gameView.PlayerTransform.localPosition += new Vector3(horizontalInput, verticalInput, 0).normalized * Time.fixedDeltaTime * _gameView.PlayerSpeed;
    }

    private void _ReceiveAiMovement(Vector2 movement)
    {
        _currentAiMovement = movement;
        Update();
    }

    private void _AiControl(Vector3 movement)
    {
        var offset = movement.normalized;
        _gameView.PlayerTransform.localPosition += offset * Time.fixedDeltaTime * _gameView.PlayerSpeed;
    }

    private void _OutOfBound()
    {        
        if (!_isIntroAnimationEnd || _isGameOver)
        {
            return;
        }
        _dodgeAgent.OutOfBound(_playerLifes);

        _playerLifes = 0;
        _gameView.SetPlayerLifes(_playerLifes);

        _GameOver();
    }

    private void _GameOver()
    {
        _isGameOver = true;
        _dodgeAgent.EndEpisode();

        if (Main.IsTraining)
        {
            _Initialize();
        }
        else
        {
            _gameView.ShowGameOver();
        }
    }

    private void _UpdatePlayerLifes(Item item)
    {
        if (!_isIntroAnimationEnd || _isGameOver)
        {
            return;
        }
        
        if (item.Type == Item.Types.NormalBullet ||
            item.Type == Item.Types.FastBullet ||
            item.Type == Item.Types.TracingBullet)
        {
            _playerLifes--;
            _hitCounter++;
            _dodgeAgent.Hit();
        }
        else if (item.Type == Item.Types.OneUp)
        {
            _playerLifes++;
            _oneUpCounter++;
            _dodgeAgent.GetOneUp();
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
