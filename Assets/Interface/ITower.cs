

public interface ITower : IMonoObj, IOwner
{
    public void Init(ITileMap tile, ITowerSO data);
    public ITowerSO Data{get;}
    public ITileMap CurTile{get;}
}