using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BorderTrainer : Trainer
{
    #region Public const
    public const int EPISODE_LENGTH = 120;
    #endregion

    #region Private properties
    private Bounds _borderBounds;
    #endregion
    public BorderTrainer(Bounds borderBounds)
    {
        _borderBounds = borderBounds;
    }

    public override int GetInitPlayerLifes()
    {
        return 1;
    }

    public override Vector3 GetInitPlayerPosition()
    {
        var type = Random.Range(0f, 1f);
        var pos = Random.Range(-0.99f, 0.99f);
        var coefX = pos;
        var coefY = pos;
        if (type < 0.25f)
        {
            coefX = 0.99f;
        }
        else if (type < 0.5f)
        {
            coefX = -0.99f;
        }
        else if (type < 0.75f)
        {
            coefY = 0.99f;
        }
        else
        {
            coefY = -0.99f;
        }
        return new Vector3(_borderBounds.extents.x * coefX, _borderBounds.extents.y * coefY, 0);
    }

    public override System.Collections.Generic.IEnumerable<SpawnItemData> GetSpawnItemData(int frameCount)
    {
        return Array.Empty<SpawnItemData>();
    }

    public override bool ShouldEndEpisode(int frameCount)
    {
        if (frameCount > EPISODE_LENGTH)
        {
            return true;
        }
        return false;
    }
}
