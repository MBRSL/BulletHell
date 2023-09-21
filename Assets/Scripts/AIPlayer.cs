using System.Collections.Generic;
using UnityEngine;

public class AiPlayer
{
    #region Public properties
    public const float BORDER_REBEL_THRESHOLD = 0.2f;
    public const float CLOSE_QUATER_DODGE_THRESHOLD = 0.2f;
    #endregion

    private class Decision
    {
        public Vector3 direction;
        public float confidence;
    }

    #region Private properties
    private Vector3 _centerPosition;
    private Transform _playerTransform;
    private Bounds _playerBounds;
    private Bounds _borderBounds;
    private List<Item> _items;

    private Vector2 _borderRebelThreshold;
    private float _closeQuaterDodgeThreshold;
    #endregion

    #region Public methods
    public AiPlayer(
        Vector3 centerPosition,
        Transform playerTransform,
        Bounds playerBounds,
        Bounds borderBounds,
        List<Item> items)
    {
        _centerPosition = centerPosition;
        _playerTransform = playerTransform;
        _playerBounds = playerBounds;
        _borderBounds = borderBounds;
        _items = items;

        _borderRebelThreshold = new Vector2(
            _borderBounds.max.x * BORDER_REBEL_THRESHOLD,
            _borderBounds.max.y * BORDER_REBEL_THRESHOLD
        );
        _closeQuaterDodgeThreshold = _borderBounds.max.magnitude * CLOSE_QUATER_DODGE_THRESHOLD;
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
        return bestDecision.direction;
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
                var direction = item.View.transform.position - _playerTransform.position;
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
            direction = closestDirection,
            confidence = minDistance < 0 ? 0 : 0.2f
        };
    }

    private Decision _HomeSick()
    {
        return new Decision
        {
            direction = _centerPosition-_playerTransform.position,
            confidence = (_centerPosition-_playerTransform.position).magnitude/_borderBounds.max.magnitude * 0.5f
        };
    }

    private Decision _BorderRebel()
    {
        var upDistance    = _borderBounds.max.y - _playerTransform.position.y;
        var rightDistance = _borderBounds.max.x - _playerTransform.position.x;
        var downDistance  = _playerTransform.position.y - _borderBounds.min.y;
        var leftDistance  = _playerTransform.position.x - _borderBounds.min.x;
        if (upDistance < _borderRebelThreshold.y)
        {
            return new Decision
            {
                direction = Vector3.down,
                confidence = 1-upDistance/_borderRebelThreshold.y
            };
        }
        if (rightDistance < _borderRebelThreshold.x)
        {
            return new Decision
            {
                direction = Vector3.left,
                confidence = 1-rightDistance/_borderRebelThreshold.x
            };
        }
        if (downDistance < _borderRebelThreshold.y)
        {
            return new Decision
            {
                direction = Vector3.up,
                confidence = 1-downDistance/_borderRebelThreshold.y
            };
        }
        if (leftDistance < _borderRebelThreshold.x)
        {
            return new Decision
            {
                direction = Vector3.right,
                confidence = 1-leftDistance/_borderRebelThreshold.x
            };
        }
        return new Decision
        {
            direction = Vector3.zero,
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
                var direction = item.View.transform.position - _playerTransform.position;
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
            if (minDistance < _playerBounds.min.magnitude)
            {
                return new Decision
                {
                    direction = _playerTransform.position - closestBullet.transform.position,
                    confidence = 1
                };
            }
            else if (minDistance < _closeQuaterDodgeThreshold)
            {
                var rigidBody = closestBullet.GetComponent<Rigidbody>();
                var evadeDirection1 = Vector3.Cross(Vector3.back, rigidBody.velocity);
                var evadeDirection2 = Vector3.Cross(Vector3.forward, rigidBody.velocity);
                var evadeDirection = 
                    (_playerTransform.position + evadeDirection1 - closestBullet.transform.position).magnitude >
                    (_playerTransform.position + evadeDirection2 - closestBullet.transform.position).magnitude ?
                    evadeDirection1 :
                    evadeDirection2;

                return new Decision
                {
                    direction = evadeDirection,
                    confidence = confidence
                };
            }
        }
        return new Decision
        {
            direction = Vector3.zero,
            confidence = 0
        };
    }
    #endregion
}
