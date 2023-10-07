using UnityEngine;

public class SimpleItemTrainer : Trainer
{
    #region Public const
    public const int EPISODE_LENGTH = 500;
    #endregion

    #region Private properties
    private Bounds _itemSpawnBounds;
    #endregion
    
    #region Public methods
    public SimpleItemTrainer(
        Bounds itemSpawnBounds
    )
    {
        _itemSpawnBounds = itemSpawnBounds;
    }

    public override int GetInitPlayerLifes()
    {
        return 1;
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
            TargetPosition = GetInitPlayerPosition()
        };
    }

    public override bool ShouldSpawnItem(int frameCount)
    {
        return frameCount % EPISODE_LENGTH == 1 || frameCount % EPISODE_LENGTH == 2;
    }

    public override bool ShouldEndEpisode(int frameCount)
    {
        if (frameCount > EPISODE_LENGTH)
        {
            return true;
        }
        return false;
    }
    #endregion
}