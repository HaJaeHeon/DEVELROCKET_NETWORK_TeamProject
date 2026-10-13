using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Enemy : MultiMono, IEnemy, IdOwn, IPacketer
{
    public void Awake()
    {
        RPCEvent = new Dictionary<string, Action<PacketData>>();
    }

    public float HP { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public float MaxHP => throw new System.NotImplementedException();

    public IEnemySO Data => throw new System.NotImplementedException();

    public int IndexId { get; set;}
    public Dictionary<string, Action<PacketData>> RPCEvent { get ; set ; }

    public override void Delete()
    {
        base.Delete();
        GameManager.Instance.Container_Enemy.DeleteObj(this);
    }

    public void Init(System.Numerics.Vector2 pos)
    {
        throw new System.NotImplementedException();
    }

    [PunRPC]
    public void LoadPacket(byte[] data)
    {
        PacketData d = new(data);
        string id = d;
        RPCEvent[id](d);
    }

    public void TakeDamage(IDamageInfo dmgInfo)
    {
        throw new System.NotImplementedException();
    }
}