using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// Deep Reinforcement Learning Agent
/// </summary>
public class DRLAgent : Agent
{
    #region Public
    public const float SURVIVAL_REWARD = 0.002f;
    public event Action<Vector2> OnMoving;
    #endregion

    #region Editor data
    [SerializeField] private BufferSensorComponent _bufferSensorComponent;
    #endregion

    #region Private properties
    private Bounds _borderBounds;
    private Transform _playerTransform;
    private RewardFunction _rewardFunction;
    private IEnumerable<Item> _closestItems;
    #endregion

    #region Public method
    public void InjectData(
        Transform playerTransform,
        Bounds borderBounds,
        RewardFunction rewardFunction
    )
    {
        _playerTransform = playerTransform;
        _borderBounds = borderBounds;
        _rewardFunction = rewardFunction;
    }

    public void InjectClosestItems(IEnumerable<Item> closestItems)
    {
        _closestItems = closestItems;
    }

    public void Hit()
    {
        Academy.Instance.StatsRecorder.Add("Hits", 1, StatAggregationMethod.Sum);
        AddReward(RewardFunction.BULLET_HIT_REWARD);
    }

    public void GetOneUp()
    {
        Academy.Instance.StatsRecorder.Add("OneUps", 1, StatAggregationMethod.Sum);
        AddReward(RewardFunction.ONEUP_HIT_REWARD);
    }

    public void OutOfBound(int remainLifes)
    {
        Academy.Instance.StatsRecorder.Add("OutOfBound", 1, StatAggregationMethod.Sum);
        AddReward(RewardFunction.OUT_OF_BOUND_REWARD + RewardFunction.BULLET_HIT_REWARD * remainLifes);
    }

    public void Pass()
    {
        Academy.Instance.StatsRecorder.Add("Pass", 1, StatAggregationMethod.Sum);
    }

    public void LogWhenEpisodeEnds(int totalHits, int totalOneUps, int frames)
    {
        Academy.Instance.StatsRecorder.Add("Episode/Hits per frame", (float)totalHits/frames, StatAggregationMethod.Average);
        Academy.Instance.StatsRecorder.Add("Episode/OneUps per frame", (float)totalOneUps/frames, StatAggregationMethod.Average);
    }
    #endregion

    #region Unity functions

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var input = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"),
            0
        );
        var direction = Movement.ToClockDirection(input);
        var output = actionsOut.DiscreteActions;
        output[0] = direction;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (_playerTransform == null)
        {
            return;
        }

        var normalizedPlayerPosition = new Vector2(
            _playerTransform.localPosition.x/_borderBounds.extents.x,
            _playerTransform.localPosition.y/_borderBounds.extents.y
        );
        sensor.AddObservation(normalizedPlayerPosition);

        if (_closestItems == null)
        {
            return;
        }
        foreach (var item in _closestItems)
        {
            var normalizedItemPosition = new Vector2(
                (item.View.transform.localPosition.x-_playerTransform.localPosition.x)/_borderBounds.extents.x,
                (item.View.transform.localPosition.y-_playerTransform.localPosition.y)/_borderBounds.extents.y
            );
            // Ignore items farther than one extent of border bounds
            if (-1 > normalizedItemPosition.x || normalizedItemPosition.x > 1 ||
                -1 > normalizedItemPosition.y || normalizedItemPosition.y > 1)
            {
                continue;
            }

            var normalizedItemVelocity = new Vector2(
                item.View.Rigidbody.velocity.x/_borderBounds.extents.x,
                item.View.Rigidbody.velocity.y/_borderBounds.extents.y
            );

            var msg = new float[]{
                // Enum should be encoded to one hot vector to declare that they are not numerically relevent
                // Unfortunately, BufferSensor doesn't support one hot vector so we can only do it this way
                item.Type == Item.Types.NormalBullet ? 1f : 0f,
                item.Type == Item.Types.FastBullet ? 1f : 0f,
                item.Type == Item.Types.TracingBullet ? 1f : 0f,
                item.Type == Item.Types.OneUp ? 1f : 0f,
                normalizedItemPosition.x,
                normalizedItemPosition.y,
                normalizedItemVelocity.x,
                normalizedItemVelocity.y
            };
            _bufferSensorComponent.AppendObservation(msg);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        OnMoving?.Invoke(Movement.ClockDirection[actions.DiscreteActions[0]]);
        AddReward(_rewardFunction.GetVariableRewardDiff(_playerTransform.localPosition, _closestItems));
        AddReward(SURVIVAL_REWARD);
    }
    #endregion
}
