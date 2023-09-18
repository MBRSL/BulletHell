using System.Collections.Generic;
using UnityEngine;

public class AiPlayer
{
    #region Public properties
    public const float BORDER_REBEL_THRESHOLD = 0.2f;
    public const float CLOSE_QUATER_DODGE_THRESHOLD = 0.1f;
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
    private List<Collidable> _bullets;
    private List<Collidable> _extends;

    private Vector2 _borderRebelThreshold;
    private float _closeQuaterDodgeThreshold;
    #endregion

    #region Public methods
    public AiPlayer(
        Vector3 centerPosition,
        Transform playerTransform,
        Bounds playerBounds,
        List<Collidable> bullets,
        List<Collidable> extends)
    {
        _centerPosition = centerPosition;
        _playerTransform = playerTransform;
        _playerBounds = playerBounds;
        _bullets = bullets;
        _extends = extends;

        _borderRebelThreshold = new Vector2(
            _playerBounds.max.x * BORDER_REBEL_THRESHOLD,
            _playerBounds.max.y * BORDER_REBEL_THRESHOLD
        );
        _closeQuaterDodgeThreshold = _playerBounds.max.magnitude * CLOSE_QUATER_DODGE_THRESHOLD;
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
        foreach (var extend in _extends)
        {
            var direction = extend.transform.position - _playerTransform.position;
            var distance = direction.magnitude;
            if (minDistance < 0 || distance < minDistance)
            {
                minDistance = distance;
                closestDirection = direction;
            }
        }
        return new Decision
        {
            direction = closestDirection,
            confidence = minDistance < 0 ? 0 : 0.5f
        };
    }

    private Decision _HomeSick()
    {
        return new Decision
        {
            direction = _centerPosition-_playerTransform.position,
            confidence = (_centerPosition-_playerTransform.position).magnitude/_playerBounds.max.magnitude * 0.5f
        };
    }

    private Decision _BorderRebel()
    {
        var upDistance    = _playerBounds.max.y - _playerTransform.position.y;
        var rightDistance = _playerBounds.max.x - _playerTransform.position.x;
        var downDistance  = _playerTransform.position.y - _playerBounds.min.y;
        var leftDistance  = _playerTransform.position.x - _playerBounds.min.x;
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
        Collidable closestBullet = null;
        foreach (var bullet in _bullets)
        {
            var direction = bullet.transform.position - _playerTransform.position;
            var distance = direction.magnitude;
            if (minDistance < 0 || distance < minDistance)
            {
                minDistance = distance;
                closestBullet = bullet;
            }
        }
        if (closestBullet != null && minDistance < _closeQuaterDodgeThreshold)
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
                confidence = 0.9f
            };
        }
        return new Decision
        {
            direction = Vector3.zero,
            confidence = 0
        };
    }
    #endregion
}
