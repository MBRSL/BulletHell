using System.Collections.Generic;
using UnityEngine;

public class IncreasingItemTrainer : Trainer
{
    #region Public const
    // Probability of spawning bullets is increased with frame count.
    // It spawns bullet every frame when reaching this number
    public const int MAX_BULLET_FRAME = 10000;
    #endregion

    #region Private properties
    private Bounds _itemSpawnBounds;
    private Bounds _borderBounds;
    private Vector3? _playerInitPosition;
    private List<SpawnItemData> _itemDatas;
    #endregion

    #region Public methods
    public IncreasingItemTrainer(
        Bounds itemSpawnBounds,
        Bounds borderBounds        
    )
    {
        _itemSpawnBounds = itemSpawnBounds;
        _borderBounds = borderBounds;
        _playerInitPosition = null;
        _itemDatas = new List<SpawnItemData>();
    }

    public override int GetInitPlayerLifes()
    {
        return 30;
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
        return Random.Range(0f, 1f) < Mathf.Pow((float)frameCount/MAX_BULLET_FRAME, 0.8f);
    }

    public override bool ShouldEndEpisode(int frameCount)
    {
        return false;
    }
   #endregion
}