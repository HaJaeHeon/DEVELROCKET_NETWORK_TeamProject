public interface ITowerSO : ISOData
{
    public float Cost{get;}
    public string Name{get;}
    public string Desc{get;}
    public string GetDetail();
    public TowerType Type{get;}

}
public enum TowerType
{
    Attack,
    Buff,
    Debuff,
    Special
}