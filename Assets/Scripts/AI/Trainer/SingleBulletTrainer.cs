using System.Collections.Generic;
using UnityEngine;

public class SimpleItemTrainer : Trainer
{
    #region Public const
    public const int EPISODE_LENGTH = 500;
    #endregion

    #region Private properties
    private Bounds _itemSpawnBounds;
    private List<SpawnItemData> _spawnItemData;
    #endregion
    
    #region Public methods
    public SimpleItemTrainer(
        Bounds itemSpawnBounds
    )
    {
        _itemSpawnBounds = itemSpawnBounds;
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
        if (frameCount == 1)
        {
            for (int i = 0; i < 2; i++)
            {
                var theta = Random.Range(0f, 2*Mathf.PI);
                _spawnItemData.Add(new SpawnItemData
                {
                    Type = _GetGameRandomItemType(),
                    InitPosition = _itemSpawnBounds.extents.magnitude * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0),
                    TargetPosition = GetInitPlayerPosition()
                });
            }
        }
        return _spawnItemData;
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