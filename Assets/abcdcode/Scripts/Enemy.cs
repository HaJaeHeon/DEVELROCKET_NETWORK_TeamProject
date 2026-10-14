using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Enemy : MultiMono, IEnemy, IdOwn, IPacketer
{
    public void Awake()
    {
        RPCEvent = new Dictionary<string, Action<PacketData>>();
        RPCEvent[ModifyHP] = ModifyHPEvent;
        IndexId = photonView.ViewID;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void RegisterPackets()
    {
        MainGamePacketer.Register(EnemyDestroy, EnemyDestroyEvent);
    }
    private float m_hp;
    public float HP {
        get
        {
            return m_hp;
        }
        set
        {
            m_hp = Mathf.Clamp(value,0,MaxHP);
            if(m_hp <= 0)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                }
            }
        }
    }

    public float MaxHP => Data.HP;

    public IEnemySO Data {get;private set;}

    public int IndexId { get; set;}
    public Dictionary<string, Action<PacketData>> RPCEvent { get ; set ; }

    public override void Delete()
    {
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
        PacketData d = new PacketData();
        d.Write(ModifyHP);
        d.Write(dmgInfo.Dmg);
        IPacketer.SendPacket(photonView,d);
    }
    public void ModifyHPEvent(PacketData data)
    {
        float dmg = data;
        HP -= dmg;
    }
    public static void EnemyDestroyEvent(PacketData data)
    {
        var e = GameManager.Instance.Container_Enemy.GetList().FindEnemy(data);
        if(e == null) return;
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(e.gameObject);
        }
        GameManager.Instance.Container_Enemy.DeleteObj(e);
    }
    private const string ModifyHP = "ModifyHP";
    public const string EnemyDestroy = "EnemyDestroy";
}