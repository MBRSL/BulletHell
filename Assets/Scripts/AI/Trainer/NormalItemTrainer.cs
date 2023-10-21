using System.Collections.Generic;
using UnityEngine;

public class NormalItemTrainer : Trainer
{
    #region Private properties
    private Bounds _itemSpawnBounds;
    private Bounds _borderBounds;
    private Transform _playerTransform;
    private List<SpawnItemData> _spawnItemData;
    #endregion

    #region Public methods
    public NormalItemTrainer(
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
        return 10;
    }

    public override Vector3 GetInitPlayerPosition()
    {
        return Vector3.zero;
    }

    public override IEnumerable<SpawnItemData> GetSpawnItemData(int frameCount)
    {
        _spawnItemData.Clear();

        if (frameCount % 30 == 0)
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