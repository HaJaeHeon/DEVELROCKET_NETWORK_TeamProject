using Photon.Pun;

public interface IPacketer
{
    public void SavePacket(PacketData data);
    public void LoadPacket(PacketData data);
    public static void SendPacket(PhotonView view, PacketData data, RpcTarget target = RpcTarget.All)
    {
        if(data.IsEmpty()) return; // 보낼 패킷이 0이면 실행안함
        view.RPC("LoadPacket",target,data.GetData());
    }
}