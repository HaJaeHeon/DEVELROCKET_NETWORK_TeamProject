using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Photon.Pun;
using UnityEngine;

public class Tower : MultiMono,ITower,IPacketer,IdOwn
{
    public void Awake()
    {
        RPCEvent = new Dictionary<string, Action<PacketData>>();
        Data = mData;
        AIData = new LookAtAndAttackAI();
        AIData.Init(this);
        IndexId = photonView.ViewID;
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void RegisterPackets()
    {
        MainGamePacketer.Register(TowerDestroy, TowerDestroyEvent);
    }
    public void Init(ITileMap tile, ITowerSO data)
    {
        CurTile = tile;
        Position = tile.Pos;
        Data = data;
        AIData = (data as TowerData).CreateAI();
        AIData.Init(this);
    }
    public virtual void OnClickDemo()
    {
        if(photonView.IsMine)
        {
            this.Delete();
        }
        else
        {
            UnityEngine.Debug.Log("다른 사람의 타워는 철거할 수 없습니다.");
        }
    }
    public override void Delete()
    {
        base.Delete();
        GameManager.Instance.Container_Tower?.DeleteObj(this);
    }
    public void Update()
    {
        AIData.GameUpdate();
    }
    [PunRPC]
    public void LoadPacket(byte[] data)
    {
        PacketData d = new(data);
        string id = d;
        RPCEvent[id](d);
    }

    

    public float Angle
    {
        get
        {
            return mainTransform.eulerAngles.z;
        }
        set
        {
            mainTransform.eulerAngles = new Vector3(0,0,value);
        }
    }
    [SerializeField]private Transform mainTransform;
    public ITowerSO Data{get;private set;}
    public TowerAI AIData{get;private set;}
    public string Owner{get;private set;}
    public Dictionary<string, Action<PacketData>> RPCEvent {get;set;}

    public ITileMap CurTile {get;private set;}
    public int IndexId { get; set; }

    public static void TowerDestroyEvent(PacketData data)
    {
        var t = GameManager.Instance.Container_Tower.GetList().FindTower(data);
        if(t == null) return;
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(t.gameObject);
        }
        GameManager.Instance.Container_Tower.DeleteObj(t);
    }

    [SerializeField]private TowerData mData;
    public const string TowerDestroy = "TowerDestroy";
}