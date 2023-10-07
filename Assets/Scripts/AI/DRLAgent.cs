using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

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
    private RuleBasedAgent _ruleBasedAgent;
    private Bounds _borderBounds;
    private Transform _playerTransform;
    private RewardFunction _rewardFunction;
    #endregion

    #region Public method
    public int MaxNumObservables { get { return _bufferSensorComponent.MaxNumObservables; } }

    public void InjectData(
        RuleBasedAgent ruleBasedAgent,
        Transform playerTransform,
        Bounds borderBounds,
        RewardFunction rewardFunction
    )
    {
        _ruleBasedAgent = ruleBasedAgent;
        _playerTransform = playerTransform;
        _borderBounds = borderBounds;
        _rewardFunction = rewardFunction;
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
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");
        var input = horizontalInput != 0 || verticalInput != 0 ?
            new Vector3(horizontalInput, verticalInput, 0) :
            _ruleBasedAgent.GetAction();
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

        var closestItems = _rewardFunction.GetClosestItems(_playerTransform.localPosition);
        foreach (var item in closestItems)
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
        AddReward(_rewardFunction.GetVariableRewardDiff(_playerTransform.localPosition));
        AddReward(SURVIVAL_REWARD);
    }
    #endregion
}
