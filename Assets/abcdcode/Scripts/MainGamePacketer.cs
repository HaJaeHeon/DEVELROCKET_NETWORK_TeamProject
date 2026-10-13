using System;
using System.Collections.Generic;
using Photon.Pun;

public class MainGamePacketer : MonoBehaviourPun, IPacketer
{
    public void Awake()
    {
        RPCEvent = new Dictionary<string, Action<PacketData>>();
        RPCEvent[Bullet.BulletHit] = Bullet.BulletHitExCute;
    }
    public Dictionary<string, Action<PacketData>> RPCEvent {get; set;}

    [PunRPC]
    public void LoadPacket(byte[] data)
    {
        PacketData d = new(data);
        string id = d;
        RPCEvent[id](d);
    }
}