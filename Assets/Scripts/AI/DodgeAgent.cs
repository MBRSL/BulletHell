using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class DodgeAgent : Agent
{
    #region Public
    public const float SURVIVAL_REWARD = 0.002f;
    public event Action<Vector2> OnMoving;
    public RewardFunction RewardSource;
    #endregion

    #region Editor data
    [SerializeField] private BufferSensorComponent _bufferSensorComponent;
    #endregion

    #region Private properties
    private OldDodgeAgent _oldDodgeAgent;
    private Bounds _borderBounds;
    private Transform _playerTransform;
    private List<Item> _items;
    #endregion

    #region Public method
    public void InjectData(
        OldDodgeAgent oldDodgeAgent,
        Transform playerTransform,
        Bounds borderBounds,
        float playerColliderRadius,
        List<Item> items)
    {
        _oldDodgeAgent = oldDodgeAgent;
        _playerTransform = playerTransform;
        _borderBounds = borderBounds;
        _items = items;

        RewardSource = new RewardFunction(
            _borderBounds,
            playerColliderRadius,
            null
        );
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
    #endregion

    #region Unity functions

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        /*
        var output = actionsOut.DiscreteActions;
        output[0] = Movement.NotMoving;
        */
        var direction = Movement.ToClockDirection(_oldDodgeAgent.GetAction());
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

        RewardSource._items = _items
            .OrderBy(i => (i.View.transform.localPosition-_playerTransform.localPosition).magnitude)
            .Take(_bufferSensorComponent.MaxNumObservables);
        foreach (var item in RewardSource._items)
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
        AddReward(RewardSource.GetVariableRewardDiff(_playerTransform.localPosition));
        AddReward(SURVIVAL_REWARD);
    }
    #endregion
}
