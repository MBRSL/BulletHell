using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardFunction
{
    #region Public const
    public const float OUT_OF_BOUND_REWARD = -1f;
    public const float BULLET_HIT_REWARD = -0.1f;
    public const float NORMAL_BULLET_THREAT = 1f;
    public const float TRACING_BULLET_THREAT = 1.2f;
    public const float FAST_BULLET_THREAT = 2f;
    public const float ONEUP_HIT_REWARD = 0.2f;
    #endregion

    #region Public properties
    public float PrevBulletDistance;
    public float PrevOneUpDistance;
    public float PrevBorderDistance;
    #endregion

    #region Private properties
    private Bounds _borderBounds;
    private float _playerColliderRadius;
    private List<Item> _items;
    private int _maxNumObservables;
    #endregion

    #region Public functions
    public RewardFunction(
        Bounds borderBounds,
        float playerColliderRadius,
        List<Item> items,
        int maxNumObservables
    )
    {
        _borderBounds = borderBounds;
        _playerColliderRadius = playerColliderRadius;
        _items = items;
        _maxNumObservables = maxNumObservables;
    }

    public IEnumerable<Item> GetClosestItems(Vector3 position)
    {
        return _items
            .OrderBy(i => (i.View.transform.localPosition-position).magnitude)
            .Take(_maxNumObservables);
    }

    public float GetVariableRewardDiff(Vector3 position)
    {
        var closestItems = GetClosestItems(position);
        var borderDistance = _GetBorderReward(position);
        var bulletDistance = _GetBulletReward(position, closestItems);
        var oneUpDistance = _GetOneUpReward(position, closestItems);
        float reward = 
            0.3f * OUT_OF_BOUND_REWARD * (borderDistance - PrevBorderDistance) +
            0.3f * BULLET_HIT_REWARD * (bulletDistance - PrevBulletDistance) +
            0.3f * ONEUP_HIT_REWARD * (oneUpDistance - PrevOneUpDistance);

        PrevBorderDistance = borderDistance;
        PrevBulletDistance = bulletDistance;
        PrevOneUpDistance = oneUpDistance;
        return reward;
    }

    public float GetVariableReward(Vector3 position)
    {
        var closestItems = GetClosestItems(position);
        var borderDistance = _GetBorderReward(position);
        var bulletDistance = _GetBulletReward(position, closestItems);
        var oneUpDistance = _GetOneUpReward(position, closestItems);
        float reward = 
            0.3f * OUT_OF_BOUND_REWARD * borderDistance +
            0.3f * BULLET_HIT_REWARD * bulletDistance +
            0.3f * ONEUP_HIT_REWARD * oneUpDistance;
        return reward;
    }
    #endregion

    #region Private functions
    private float _GetBulletReward(Vector3 position, IEnumerable<Item> closestItems)
    {
        if (closestItems == null)
        {
            return 0;
        }

        var sum = 0f;
        foreach (var item in closestItems)
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
                // Piriform shape
                // https://mathworld.wolfram.com/PiriformSurface.html
                var theta = Mathf.PI-Mathf.Atan2(item.View.Rigidbody.velocity.y, item.View.Rigidbody.velocity.x);
                // Adjust the function shape to fit our scenario
                var scalerX = 0.07f;
                var scalerY = 0.2f;
                // When alpha = this number, z has maximum of 1
                var alpha = 3.079f;
                // Maximum is at x = 3/4*alpha
                var vec = position-item.View.transform.localPosition;
                var xPrime = (vec.x*Mathf.Cos(theta)-vec.y*Mathf.Sin(theta)) * scalerX + 3f/4 * alpha;
                var yPrime = (vec.y*Mathf.Cos(theta)+vec.x*Mathf.Sin(theta)) * scalerY;
                var x2 = xPrime*xPrime;
                var x3 = xPrime*x2;
                var x4 = xPrime*x3;

                var value = Mathf.Max(0, -x4/alpha/alpha + x3/alpha - yPrime*yPrime);
                sum += threatCoef * Mathf.Pow(value, 3f);
            }
        }
        return sum;
    }

    private float _GetOneUpReward(Vector3 position, IEnumerable<Item> closestItems)
    {
        if (closestItems == null)
        {
            return 0;
        }

        var sum = 0f;
        foreach (var item in closestItems)
        {
            if (item.Type == Item.Types.OneUp)
            {
                var distance = (item.View.transform.localPosition-position).magnitude;
                var distanceWithoutBounds = Mathf.Max(0, distance-_playerColliderRadius*4);
                sum += Mathf.Pow(1f/(distanceWithoutBounds+1), 1.5f);
            }
        }
        return sum;
    }

    private float _GetBorderReward(Vector3 position)
    {
        var dx = Mathf.Abs(position.x/_borderBounds.extents.x);
        var dy = Mathf.Abs(position.y/_borderBounds.extents.y);
        return Mathf.Max(Mathf.Pow(dx, 10), Mathf.Pow(dy, 10));
    }
    #endregion
}