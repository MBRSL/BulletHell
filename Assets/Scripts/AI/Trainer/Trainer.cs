using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to setup different training environments.
/// Like no bullets, few bullets, lots of bullets, etc.
/// </summary>
public abstract class Trainer
{
    #region public class
    public class SpawnItemData
    {
        public Item.Types Type;
        public Vector3 InitPosition;
        public Vector3 TargetPosition;
    }

    #endregion

    #region public methods
    public abstract int GetInitPlayerLifes();
    public abstract Vector3 GetInitPlayerPosition();
    public abstract IEnumerable<SpawnItemData> GetSpawnItemData(int frameCount);
    public abstract bool ShouldEndEpisode(int frameCount);
    #endregion

    #region Protected methods
    protected Item.Types _GetGameRandomItemType()
    {
        float typeRnd = Random.Range(0f, 1f);
        if (typeRnd < 0.1f)
        {
            return Item.Types.TracingBullet;
        }
        else if (typeRnd < 0.3f)
        {
            return Item.Types.FastBullet;
        }
        else if (typeRnd < 0.985f)
        {
            return Item.Types.NormalBullet;
        }
        else
        {
            return Item.Types.OneUp;
        }
    }
    #endregion
}
