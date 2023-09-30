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
    public const float OUT_OF_BOUND_REWARD = -1f;
    public const float BULLET_HIT_REWARD = -0.2f;
    public const float NORMAL_BULLET_THREAT = 1f;
    public const float TRACING_BULLET_THREAT = 1.2f;
    public const float FAST_BULLET_THREAT = 2f;
    public const float ONEUP_HIT_REWARD = 0.2f;
    public const float SURVIVAL_REWARD = 0.01f;
    public event Action<Vector2> OnMoving;
    public Reward RewardSource;

    public class Reward
    {
        public float PrevBulletDistance;
        public float PrevOneUpDistance;
        public float PrevBorderDistance;
        private Transform _playerTransform;
        private Bounds _borderBounds;
        private float _playerColliderRadius;
        internal IEnumerable<Item> _items;

        public Reward(
            Transform playerTransform,
            Bounds borderBounds,
            float playerColliderRadius,
            IEnumerable<Item> items
        )
        {
            _playerTransform = playerTransform;
            _borderBounds = borderBounds;
            _playerColliderRadius = playerColliderRadius;
            _items = items;
        }

        public float GetVariableReward()
        {
            var borderDistance = _GetBorderReward();
            var bulletDistance = _GetBulletReward();
            var oneUpDistance = _GetOneUpReward();
            float reward = 
                0.3f * OUT_OF_BOUND_REWARD * (borderDistance - PrevBorderDistance) +
                0.3f * BULLET_HIT_REWARD * (bulletDistance - PrevBulletDistance) +
                0.3f * ONEUP_HIT_REWARD * (oneUpDistance - PrevOneUpDistance);

            PrevBorderDistance = borderDistance;
            PrevBulletDistance = bulletDistance;
            PrevOneUpDistance = oneUpDistance;
            return reward;
        }

        private float _GetBulletReward()
        {
            if (_items == null)
            {
                return 0;
            }

            var sum = 0f;
            foreach (var item in _items)
            {
                var threatCoef = 0f;
                if (item.Type == Item.Types.NormalBullet)
                {
                    threatCoef = NORMAL_BULLET_THREAT;
                }
                else if (item.Type == Item.Types.FastBullet)
                {
                    threatCoef = FAST_BULLET_THREAT;
                }
                else if (item.Type == Item.Types.TracingBullet)
                {
                    threatCoef = TRACING_BULLET_THREAT;
                }

                if (threatCoef > 0)
                {
                    var distance = (item.View.transform.localPosition-_playerTransform.localPosition).magnitude;
                    var distanceWithoutBounds = Mathf.Max(0, distance-_playerColliderRadius);

                    var bulletToPlayer = _playerTransform.localPosition-item.View.transform.localPosition;
                    var velocitySimilarity = Vector3.Dot(bulletToPlayer.normalized, item.View.Rigidbody.velocity.normalized);
                    // Ignore bullets that are already behind player
                    velocitySimilarity = Mathf.Max(0, velocitySimilarity);
                    // Distance is always a threat, can accidentlly collide to it even if it's behind
                    sum += threatCoef/(distanceWithoutBounds+1)*(1+velocitySimilarity)/2;
                }
            }
            return sum;
        }

        private float _GetOneUpReward()
        {
            if (_items == null)
            {
                return 0;
            }

            var sum = 0f;
            foreach (var item in _items)
            {
                if (item.Type == Item.Types.OneUp)
                {
                    var distance = (item.View.transform.localPosition-_playerTransform.localPosition).magnitude;
                    var distanceWithoutBounds = Mathf.Max(0, distance-_playerColliderRadius);
                    sum += 1f/(distanceWithoutBounds+1);
                }
            }
            return sum;
        }

        public float _GetBorderReward()
        {
            var dx = Mathf.Abs(_playerTransform.localPosition.x/_borderBounds.extents.x);
            var dy = Mathf.Abs(_playerTransform.localPosition.y/_borderBounds.extents.y);
            return Mathf.Max(Mathf.Pow(dx, 10), Mathf.Pow(dy, 10));
        }
    }
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

        RewardSource = new Reward(
            _playerTransform,
            _borderBounds,
            playerColliderRadius,
            null
        );
    }

    public void Hit()
    {
        Academy.Instance.StatsRecorder.Add("Hits", 1, StatAggregationMethod.Sum);
        AddReward(BULLET_HIT_REWARD);
    }

    public void GetOneUp()
    {
        Academy.Instance.StatsRecorder.Add("OneUps", 1, StatAggregationMethod.Sum);
        AddReward(ONEUP_HIT_REWARD);
    }

    public void OutOfBound()
    {
        Academy.Instance.StatsRecorder.Add("OutOfBound", 1, StatAggregationMethod.Sum);
        SetReward(OUT_OF_BOUND_REWARD);
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
        AddReward(RewardSource.GetVariableReward());
        AddReward(SURVIVAL_REWARD);
    }
    #endregion
}
