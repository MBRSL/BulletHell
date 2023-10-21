using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// This class controls the main loop of game, including game logic, scores, spawing items.
/// </summary>
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
    private DRLAgent _drlAgent;
    private RuleBasedAgent _ruleBasedAgent;
    private List<Item> _items;
    private RewardFunction _rewardFunction;
    
    private Trainer _trainer;
    private bool _isIntroAnimationEnd;
    private bool _isGameOver;
    private int _frameCounter;
    private int _hitCounter;
    private int _oneUpCounter;
    private int _playerLifes;
    private float _defaultTrainingMode;
    private bool _useDrlAgent;
    private int _maxNumObservables;
    #endregion

    #region Public methods
    public GameManager(
        GameView gameView,
        DRLAgent drlAgent,
        float defaultTrainingMode,
        bool useDrlAgent,
        int maxNumObservables
    )
    {
        _gameView = gameView;
        _gameView.OnItemHit += _UpdatePlayerLifes;
        _gameView.OnItemOutOfBounds += _RemoveItem;
        _gameView.OnPlayerOutOfBounds += _OutOfBound;
        _gameView.OnClickRetry += _Initialize;
        _gameView.OnIntroAnimationEnd += _IntroAnimationEnded;

        _items = new List<Item>();

        _rewardFunction = new RewardFunction(
            _gameView.BorderBounds,
            _gameView.PlayerBounds.extents.magnitude
        );

        _ruleBasedAgent = new RuleBasedAgent
        (
            _gameView.PlayerBounds,
            _gameView.BorderBounds
        );

        _drlAgent = drlAgent;
        _drlAgent.OnMoving += _MovePlayer;
        _drlAgent.InjectData
        (
            _gameView.PlayerTransform,
            _gameView.BorderBounds,
            _rewardFunction
        );

        _defaultTrainingMode = Academy.Instance.EnvironmentParameters.GetWithDefault("trainingMode", defaultTrainingMode);
        _useDrlAgent = useDrlAgent;
        _maxNumObservables = maxNumObservables;
        
        _Initialize();
    }

    public void Update()
    {
        if (!_isIntroAnimationEnd || _isGameOver)
        {
            return;
        }

        _frameCounter++;

        var closestItems = _items
            .OrderBy(i => (i.View.transform.localPosition-_gameView.PlayerTransform.localPosition).magnitude)
            .Take(_maxNumObservables)
            .ToArray();

        _UpdatePlayerPosition(closestItems);

        _gameView.UpdateInfo(
            _frameCounter,
            _CalculateScore(_frameCounter),
            _rewardFunction.PrevBulletDistance,
            _rewardFunction.PrevOneUpDistance,
            _rewardFunction.PrevBorderDistance,
            _hitCounter,
            _oneUpCounter,
            _drlAgent.GetCumulativeReward(),
            _trainer.ToString(),
            _playerLifes);
        _gameView.UpdateRewardVisualizer(_rewardFunction, closestItems);

        _UpdateItems(_frameCounter);

        if (_trainer.ShouldEndEpisode(_frameCounter))
        {
            _drlAgent.Pass();
            _drlAgent.EndEpisode();
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
            _trainer = new SimpleItemTrainer(_gameView.ItemSpawnBounds);
        }
        else if (mode <= 2)
        {
            _trainer = new NormalItemTrainer(_gameView.ItemSpawnBounds, _gameView.BorderBounds, _gameView.PlayerTransform);
        }
        else
        {
            _trainer = new IncreasingItemTrainer(_gameView.ItemSpawnBounds, _gameView.BorderBounds, _gameView.PlayerTransform);
        }
    }

    private void _IntroAnimationEnded()
    {
        _isIntroAnimationEnd = true;
        _drlAgent.OnEpisodeBegin();
    }

    private int _CalculateScore(int frameCount)
    {
        return (int)Mathf.Pow(frameCount, 1.05f);
    }

    private void _UpdatePlayerPosition(IEnumerable<Item> closestItems)
    {
        // User inputs can override agents' action
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input != Vector2.zero)
        {
            var direction = Movement.SnapToClockDirection(input);
            _MovePlayer(direction);
        }
        else
        {
            if (Environment.IsTraining || _useDrlAgent)
            {
                // We need to feed the current items for agents to make decision
                // But unfortunately we cannot pass it to Unity's CollectObservations() so we have to inject here
                _drlAgent.InjectClosestItems(closestItems);
                // This triggers Unity's OnActionReceived(), and then triggers _MovePlayer() in this class
                _drlAgent.RequestDecision();
            }
            else
            {
                Vector3 direction = _ruleBasedAgent.RequestAction(_gameView.PlayerTransform.localPosition, closestItems);
                _MovePlayer(direction);
            }
        }
    }

    private void _UpdateItems(int frameCount)
    {
        var spawnItemDatas = _trainer.GetSpawnItemData(frameCount);
        foreach (var spawnItemData in spawnItemDatas)
        {
            var item = _gameView.SpawnItem(spawnItemData.Type, spawnItemData.InitPosition, spawnItemData.TargetPosition);
            _items.Add(item);
        }
        _gameView.UpdateTracingBullet(_items);
    }

    private void _MovePlayer(Vector2 movement)
    {
        _gameView.MovePlayer(movement);
    }

    private void _OutOfBound()
    {        
        if (!_isIntroAnimationEnd || _isGameOver)
        {
            return;
        }
        _drlAgent.OutOfBound(_playerLifes);

        _playerLifes = 0;
        _gameView.SetPlayerLifes(_playerLifes);

        _GameOver();
    }

    private void _GameOver()
    {
        _isGameOver = true;
        _drlAgent.EndEpisode();

        if (Environment.IsTraining)
        {
            _Initialize();
        }
        else
        {
            _gameView.ShowGameOverFxAndUi();
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
            _drlAgent.Hit();
        }
        else if (item.Type == Item.Types.OneUp)
        {
            _playerLifes++;
            _oneUpCounter++;
            _drlAgent.GetOneUp();
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
