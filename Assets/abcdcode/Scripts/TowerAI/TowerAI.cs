public abstract class TowerAI : IPacketer
{
    public virtual void Init(Tower parent)
    {
        Parent = parent;
    }

    public void SavePacket(PacketData data)
    {
    }

    public void LoadPacket(PacketData data)
    {
    }

    public Tower Parent{get;set;}
}