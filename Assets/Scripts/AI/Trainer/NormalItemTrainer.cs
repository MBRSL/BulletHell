using UnityEngine;

public class NormalItemTrainer : Trainer
{
    #region Private properties
    private Bounds _itemSpawnBounds;
    private Bounds _borderBounds;
    private Transform _playerTransform;
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
        return new SpawnItemData
        {
            Type = type,
            InitPosition = _itemSpawnBounds.extents.magnitude * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0),
            TargetPosition = targetPosition
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