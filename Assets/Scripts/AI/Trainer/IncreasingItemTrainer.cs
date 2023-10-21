using System.Collections.Generic;
using UnityEngine;

public class IncreasingItemTrainer : Trainer
{
    #region Public const
    // Probability of spawning items is increased with frame count.
    // Items are spawned for sure every frame when reaching this number
    public const int MAX_BULLET_FRAME = 2000;
    #endregion

    #region Private properties
    private Bounds _itemSpawnBounds;
    private Bounds _borderBounds;
    private Transform _playerTransform;
    private List<SpawnItemData> _spawnItemData;
    #endregion

    #region Public methods
    public IncreasingItemTrainer(
        Bounds itemSpawnBounds,
        Bounds borderBounds,
        Transform playerTransform     
    )
    {
        _itemSpawnBounds = itemSpawnBounds;
        _borderBounds = borderBounds;
        _playerTransform = playerTransform;
        _spawnItemData = new List<SpawnItemData>();
    }

    public override int GetInitPlayerLifes()
    {
        return 1;
    }

    public override Vector3 GetInitPlayerPosition()
    {
        return Vector3.zero;
    }

    public override IEnumerable<SpawnItemData> GetSpawnItemData(int frameCount)
    {
        _spawnItemData.Clear();
        for (var spawnCount = (float)frameCount/MAX_BULLET_FRAME;
             spawnCount > 1 || spawnCount > Random.Range(0f, 1f);
             spawnCount -= 1)
        {
            var type = _GetGameRandomItemType();
            var theta = Random.Range(0f, 2*Mathf.PI);
            var targetPosition = _playerTransform.localPosition;
            if (type != Item.Types.TracingBullet)
            {
                targetPosition = new Vector3(
                    _borderBounds.extents.x * Random.Range(-1f, 1f),
                    _borderBounds.extents.y * Random.Range(-1f, 1f),
                    0
                );
            }
            _spawnItemData.Add(new SpawnItemData
            {
                Type = type,
                InitPosition = _itemSpawnBounds.extents.magnitude * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0),
                TargetPosition = targetPosition
            });
        }
        return _spawnItemData;
    }

    public override bool ShouldEndEpisode(int frameCount)
    {
        return false;
    }
   #endregion
}