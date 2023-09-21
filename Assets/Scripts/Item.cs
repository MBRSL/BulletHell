public class Item
{
    #region Public
    public enum Types
    {
        NormalBullet,
        FastBullet,
        TracingBullet,
        OneUp
    }

    public Types Type;

    public ItemView View
    {
        get; private set;
    }

    public Item(Types type, ItemView view)
    {
        Type = type;
        View = view;
    }
    #endregion
}
