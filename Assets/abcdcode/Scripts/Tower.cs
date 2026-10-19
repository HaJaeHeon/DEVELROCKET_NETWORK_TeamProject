using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Tower : MultiMono,ITower,IPacketer,IdOwn,IPunInstantiateMagicCallback
{
    public void Awake()
    {
        RPCEvent = new Dictionary<string, Action<PacketData>>();
        if(m_HeadSprite != null) m_HeadDefaultColor = m_HeadSprite.color;
        Data = mData;
        AIData = new LookAtAndAttackAI();
        AIData.Init(this);
    }
    // PhotonNetwork.Instantiate 시 모든 클라이언트에서 호출 (풀에서 재사용될 때도 호출됨)
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        
        // 풀 재사용 시 Awake가 다시 호출되지 않으므로 ViewID 갱신 및 컨테이너 등록은 여기서 수행
        IndexId = photonView.ViewID;
        OwnerPlayer = null; // 풀 재사용 시 이전 소유자 초기화 (TowerSetOwner 패킷으로 지정됨)
        ApplyOwnerColor();
        GameManager.Instance?.Container_Tower.AddObj(this);
        object[] instData = info.photonView.InstantiationData;
        if(instData != null && instData.Length > 0 && instData[0] is string towerId)
        {
            InitData(GameManager.Instance.TowerDB.Get(towerId)?.data);
        }
        else
        {
            // TowerDB를 거치지 않고 소환된 경우 프리팹에 지정된 기본 데이터 사용
            InitData(mData);
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void RegisterPackets()
    {
        MainGamePacketer.Register(TowerDestroy, TowerDestroyEvent);
        MainGamePacketer.Register(TowerSetOwner, TowerSetOwnerEvent);
    }
    /// <summary>
    /// 타워 소유자를 지정하고 모든 클라이언트에 전파 (Player는 ActorNumber로 식별)
    /// </summary>
    public void SetOwner(Player owner)
    {
        PacketData d = new PacketData();
        d.Write(TowerSetOwner);
        d.Write(IndexId);
        d.Write(owner != null ? owner.ActorNumber : -1);
        IPacketer.SendPacket(GameManager.Instance.MainGamePacketer.photonView, d);
    }
    public static void TowerSetOwnerEvent(PacketData data)
    {
        int tid = data;
        int actorNumber = data;
        var t = GameManager.Instance.Container_Tower.GetList().FindTower(tid);
        if(t == null) return;
        t.OwnerPlayer = actorNumber >= 0 && PhotonNetwork.CurrentRoom != null
            ? PhotonNetwork.CurrentRoom.GetPlayer(actorNumber)
            : null;
        t.ApplyOwnerColor();
    }
    /// <summary>
    /// 소유자의 커스텀 프로퍼티(PlayerColor)를 읽어 바닥 스프라이트 색상을 변경 (소유자가 없으면 흰색)
    /// </summary>
    public void ApplyOwnerColor()
    {
        if(m_BaseSprite == null) return;
        Color color = Color.white;
        if(OwnerPlayer != null && OwnerPlayer.CustomProperties.TryGetValue(PlayerColorKey, out object value) && value is int colorValue)
        {
            color = ExtensionUtil.GetColor((MyColor)colorValue);
        }
        m_BaseSprite.color = color;
    }
    public void Init(ITileMap tile, ITowerSO data)
    {
        SetTile(tile);
        InitData(data);
    }
    public void SetTile(ITileMap tile)
    {
        CurTile = tile;
        Position = tile.Pos;
    }
    public void InitData(ITowerSO data)
    {
        if(data == null) return;
        Data = data;
        AIData = (data as TowerData).CreateAI();
        AIData.Init(this);
        ApplyLevelColor();
    }
    /// <summary>
    /// 데이터 레벨에 따라 머리 스프라이트 색상을 변경 (1레벨은 프리팹 기본 색)
    /// </summary>
    public void ApplyLevelColor()
    {
        if(m_HeadSprite == null || Data == null) return;
        m_HeadSprite.color = Data.Level switch
        {
            2 => HeadColorLv2,
            3 => HeadColorLv3,
            _ => m_HeadDefaultColor,
        };
    }
    public virtual void OnClickDemo()
    {
        if(OwnerPlayer == PhotonNetwork.LocalPlayer)
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
    public Player OwnerPlayer{get;private set;}
    public string Owner => OwnerPlayer?.NickName;
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
    [SerializeField]private SpriteRenderer m_BaseSprite;
    [SerializeField]private SpriteRenderer m_HeadSprite;
    private Color m_HeadDefaultColor = Color.white;
    private static readonly Color HeadColorLv2 = new Color32(255, 255, 128, 255);
    private static readonly Color HeadColorLv3 = new Color32(255, 255, 0, 255);
    public const string TowerDestroy = "TowerDestroy";
    public const string TowerSetOwner = "TowerSetOwner";
    public const string PlayerColorKey = "PlayerColor";
}