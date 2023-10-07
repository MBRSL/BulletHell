using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tradition agents.
/// This class include 4 smaller agents which represent different policies.
/// 1. Gullutony
///   - Tries to get Oneup so that player can survive longer.
/// 2. HomeSick
///   - Prefer to stay in the center. Because player get less choices of movement in the corner.
/// 3. BorderRebel
///   - Player dies when it touch border line. Keep away from it.
/// 4. CloseQuarterDodge
///   - Move left/right when facing bullets in close range.
///   - If it's even closer then move backward a little bit so that there is enough time to evade by moving left/right later.
/// </summary>
public class RuleBasedAgent
{
    #region Public properties
    public const float BORDER_REBEL_THRESHOLD = 0.2f;
    public const float CLOSE_QUARTER_DODGE_THRESHOLD = 0.2f;
    #endregion

    private class Decision
    {
        public int direction;
        public float confidence;
    }

    #region Private properties
    private Bounds _playerBounds;
    private Bounds _borderBounds;

    private Vector2 _borderRebelThreshold;
    private float _closeQuarterDodgeThreshold;
    #endregion

    #region Public methods
    public RuleBasedAgent(
        Bounds playerBounds,
        Bounds borderBounds
    )
    {
        _playerBounds = playerBounds;
        _borderBounds = borderBounds;

        _borderRebelThreshold = new Vector2(
            _borderBounds.extents.x * BORDER_REBEL_THRESHOLD,
            _borderBounds.extents.y * BORDER_REBEL_THRESHOLD
        );
        _closeQuarterDodgeThreshold = _borderBounds.extents.magnitude * CLOSE_QUARTER_DODGE_THRESHOLD;
    }

    public Vector3 RequestAction(Vector3 position, IEnumerable<Item> closestItems)
    {
        var bestDecision = new Decision();
        var gullutony = _Gullutony(position, closestItems);
        if (gullutony.confidence > bestDecision.confidence)
        {
            bestDecision = gullutony;
        }
        var homeSick = _HomeSick(position);
        if (homeSick.confidence > bestDecision.confidence)
        {
            bestDecision = homeSick;
        }
        var borderRebel = _BorderRebel(position);
        if (borderRebel.confidence > bestDecision.confidence)
        {
            bestDecision = borderRebel;
        }
        var closeQuarterDodge = _CloseQuarterDodge(position, closestItems);
        if (closeQuarterDodge.confidence > bestDecision.confidence)
        {
            bestDecision = closeQuarterDodge;
        }
        //Debug.Log($"Out: {bestDecision.direction}\nG: {gullutony.confidence:F2}\nH: {homeSick.confidence:F2}\nB: {borderRebel.confidence:F2}\nC: {closeQuarterDodge.confidence:F2}");
        return Movement.ClockDirection[bestDecision.direction];
    }
    #endregion

    #region Private methods
    private Decision _Gullutony(Vector3 position, IEnumerable<Item> closestItems)
    {
        var minDistance = -1f;
        var closestDirection = Vector3.zero;
        foreach (var item in closestItems)
        {
            if (item.Type == Item.Types.OneUp)
            {
                var direction = item.View.transform.localPosition - position;
                var distance = direction.magnitude;
                if (minDistance < 0 || distance < minDistance)
                {
                    minDistance = distance;
                    closestDirection = direction;
                }
            }
        }
        return new Decision
        {
            direction = Movement.ToClockDirection(closestDirection.normalized),
            confidence = minDistance < 0 ? 0 : 0.2f
        };
    }

    private Decision _HomeSick(Vector3 position)
    {
        return new Decision
        {
            direction = Movement.ToClockDirection(-position),
            confidence = position.magnitude/_borderBounds.extents.magnitude * 0.5f
        };
    }

    private Decision _BorderRebel(Vector3 position)
    {
        var upDistance    = _borderBounds.extents.y - position.y;
        var rightDistance = _borderBounds.extents.x - position.x;
        var downDistance  = position.y + _borderBounds.extents.y;
        var leftDistance  = position.x + _borderBounds.extents.x;
        if (upDistance < _borderRebelThreshold.y)
        {
            return new Decision
            {
                direction = Movement.Down,
                confidence = 1-upDistance/_borderRebelThreshold.y
            };
        }
        if (rightDistance < _borderRebelThreshold.x)
        {
            return new Decision
            {
                direction = Movement.Left,
                confidence = 1-rightDistance/_borderRebelThreshold.x
            };
        }
        if (downDistance < _borderRebelThreshold.y)
        {
            return new Decision
            {
                direction = Movement.Up,
                confidence = 1-downDistance/_borderRebelThreshold.y
            };
        }
        if (leftDistance < _borderRebelThreshold.x)
        {
            return new Decision
            {
                direction = Movement.Right,
                confidence = 1-leftDistance/_borderRebelThreshold.x
            };
        }
        return new Decision
        {
            direction = Movement.NotMoving,
            confidence = 0
        };
    }
    private Decision _CloseQuarterDodge(Vector3 position, IEnumerable<Item> closestItems)
    {
        var minDistance = -1f;
        ItemView closestBullet = null;
        foreach (var item in closestItems)
        {
            if (item.Type == Item.Types.NormalBullet ||
                item.Type == Item.Types.FastBullet ||
                item.Type == Item.Types.TracingBullet)
            {
                var direction = item.View.transform.localPosition - position;
                var distance = direction.magnitude;
                if (minDistance < 0 || distance < minDistance)
                {
                    minDistance = distance;
                    closestBullet = item.View;
                }
            }
        }
        if (closestBullet != null)
        {
            float confidence = Mathf.Pow(1-minDistance/_closeQuarterDodgeThreshold, 2);
            if (minDistance < _playerBounds.extents.magnitude)
            {
                return new Decision
                {
                    direction = Movement.ToClockDirection(position - closestBullet.transform.localPosition),
                    confidence = 1
                };
            }
            else if (minDistance < _closeQuarterDodgeThreshold)
            {
                var rigidBody = closestBullet.GetComponent<Rigidbody>();
                var evadeDirection1 = Vector3.Cross(Vector3.back, rigidBody.velocity).normalized;
                var evadeDirection2 = Vector3.Cross(Vector3.forward, rigidBody.velocity).normalized;
                var evadeDirection = 
                    (position + evadeDirection1 - closestBullet.transform.localPosition).magnitude >
                    (position + evadeDirection2 - closestBullet.transform.localPosition).magnitude ?
                    evadeDirection1 :
                    evadeDirection2;

                return new Decision
                {
                    direction = Movement.ToClockDirection(evadeDirection),
                    confidence = confidence
                };
            }
        }
        return new Decision
        {
            direction = Movement.NotMoving,
            confidence = 0
        };
    }
    #endregion
}
