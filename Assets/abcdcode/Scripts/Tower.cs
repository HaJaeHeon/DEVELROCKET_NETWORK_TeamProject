using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Photon.Pun;
using UnityEngine;

public class Tower : MultiMono,ITower,IPacketer
{
    public void Awake()
    {
        RPCEvent = new Dictionary<string, Action<PacketData>>();
        Data = mData;
        AIData = new LookAtAndAttackAI();
        AIData.Init(this);
    }
    public void Init(ITileMap tile, ITowerSO data)
    {
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
        if(PhotonNetwork.IsMasterClient)
        {
            AIData.GameUpdate();
        }
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

    [SerializeField]private TowerData mData;
}