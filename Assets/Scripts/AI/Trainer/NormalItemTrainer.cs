using System.Collections.Generic;
using UnityEngine;

public class NormalItemTrainer : Trainer
{
    #region Private properties
    private Bounds _itemSpawnBounds;
    private Bounds _borderBounds;
    #endregion

    #region Public methods
    public NormalItemTrainer(
        Bounds itemSpawnBounds,
        Bounds borderBounds
    )
    {
        _itemSpawnBounds = itemSpawnBounds;
        _borderBounds = borderBounds;
    }

    public override int GetInitPlayerLifes()
    {
        return 10;
    }

    public override Vector3 GetInitPlayerPosition()
    {
        return Vector3.zero;
    }

    public override SpawnItemData GetSpawnItemData()
    {
        var theta = Random.Range(0f, 2*Mathf.PI);
        return new SpawnItemData
        {
            Type = _GetGameRandomItemType(),
            InitPosition = _itemSpawnBounds.extents.magnitude * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0),
            TargetPosition = new Vector3(
                _borderBounds.extents.x * Random.Range(-1f, 1f),
                _borderBounds.extents.y * Random.Range(-1f, 1f),
                0
            )
        };
    }

    public override bool ShouldSpawnItem(int frameCount)
    {
        return frameCount % 30 == 0;
    }

    public override bool ShouldEndEpisode(int frameCount)
    {
        return false;
    }
    #endregion
}