using System.Collections.Generic;
using UnityEngine;

public class OldDodgeAgent
{
    #region Public properties
    public const float BORDER_REBEL_THRESHOLD = 0.2f;
    public const float CLOSE_QUATER_DODGE_THRESHOLD = 0.2f;
    #endregion

    private class Decision
    {
        public int direction;
        public float confidence;
    }

    #region Private properties
    private Transform _playerTransform;
    private Bounds _playerBounds;
    private Bounds _borderBounds;
    private List<Item> _items;

    private Vector2 _borderRebelThreshold;
    private float _closeQuaterDodgeThreshold;
    #endregion

    #region Public methods
    public OldDodgeAgent(
        Transform playerTransform,
        Bounds playerBounds,
        Bounds borderBounds,
        List<Item> items)
    {
        _playerTransform = playerTransform;
        _playerBounds = playerBounds;
        _borderBounds = borderBounds;
        _items = items;

        _borderRebelThreshold = new Vector2(
            _borderBounds.extents.x * BORDER_REBEL_THRESHOLD,
            _borderBounds.extents.y * BORDER_REBEL_THRESHOLD
        );
        _closeQuaterDodgeThreshold = _borderBounds.extents.magnitude * CLOSE_QUATER_DODGE_THRESHOLD;
    }

    public Vector3 GetAction()
    {
        var bestDecision = new Decision();
        var gullutony = _Gullutony();
        if (gullutony.confidence > bestDecision.confidence)
        {
            bestDecision = gullutony;
        }
        var homeSick = _HomeSick();
        if (homeSick.confidence > bestDecision.confidence)
        {
            bestDecision = homeSick;
        }
        var borderRebel = _BorderRebel();
        if (borderRebel.confidence > bestDecision.confidence)
        {
            bestDecision = borderRebel;
        }
        var closeQuaterDodge = _CloseQuaterDodge();
        if (closeQuaterDodge.confidence > bestDecision.confidence)
        {
            bestDecision = closeQuaterDodge;
        }
        //Debug.Log($"Out: {bestDecision.direction}\nG: {gullutony.confidence:F2}\nH: {homeSick.confidence:F2}\nB: {borderRebel.confidence:F2}\nC: {closeQuaterDodge.confidence:F2}");
        return Movement.ClockDirection[bestDecision.direction];
    }
    #endregion

    #region Private methods
    private Decision _Gullutony()
    {
        var minDistance = -1f;
        var closestDirection = Vector3.zero;
        foreach (var item in _items)
        {
            if (item.Type == Item.Types.OneUp)
            {
                var direction = item.View.transform.localPosition - _playerTransform.localPosition;
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

    private Decision _HomeSick()
    {
        return new Decision
        {
            direction = Movement.ToClockDirection(-_playerTransform.localPosition),
            confidence = _playerTransform.localPosition.magnitude/_borderBounds.extents.magnitude * 0.5f
        };
    }

    private Decision _BorderRebel()
    {
        var upDistance    = _borderBounds.extents.y - _playerTransform.localPosition.y;
        var rightDistance = _borderBounds.extents.x - _playerTransform.localPosition.x;
        var downDistance  = _playerTransform.localPosition.y + _borderBounds.extents.y;
        var leftDistance  = _playerTransform.localPosition.x + _borderBounds.extents.x;
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
    private Decision _CloseQuaterDodge()
    {
        var minDistance = -1f;
        ItemView closestBullet = null;
        foreach (var item in _items)
        {
            if (item.Type == Item.Types.NormalBullet ||
                item.Type == Item.Types.FastBullet ||
                item.Type == Item.Types.TracingBullet)
            {
                var direction = item.View.transform.localPosition - _playerTransform.localPosition;
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
            float confidence = Mathf.Pow(1-minDistance/_closeQuaterDodgeThreshold, 2);
            if (minDistance < _playerBounds.extents.magnitude)
            {
                return new Decision
                {
                    direction = Movement.ToClockDirection(_playerTransform.localPosition - closestBullet.transform.localPosition),
                    confidence = 1
                };
            }
            else if (minDistance < _closeQuaterDodgeThreshold)
            {
                var rigidBody = closestBullet.GetComponent<Rigidbody>();
                var evadeDirection1 = Vector3.Cross(Vector3.back, rigidBody.velocity).normalized;
                var evadeDirection2 = Vector3.Cross(Vector3.forward, rigidBody.velocity).normalized;
                var evadeDirection = 
                    (_playerTransform.localPosition + evadeDirection1 - closestBullet.transform.localPosition).magnitude >
                    (_playerTransform.localPosition + evadeDirection2 - closestBullet.transform.localPosition).magnitude ?
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
