using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class Enemy : MultiMono, IEnemy, IdOwn, IPacketer, IPunInstantiateMagicCallback
{
    public void Awake()
    {
        RPCEvent = new Dictionary<string, Action<PacketData>>();
        RPCEvent[ModifyHP] = ModifyHPEvent;
    }
    // PhotonNetwork.Instantiate 시 모든 클라이언트에서 호출 (풀에서 재사용될 때도 호출됨)
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // 풀 재사용 시 Awake가 다시 호출되지 않으므로 ViewID 갱신 및 컨테이너 등록은 여기서 수행
        IndexId = photonView.ViewID;
        GameManager.Instance?.Container_Enemy.AddObj(this);
        object[] instData = info.photonView.InstantiationData;
        if(instData != null && instData.Length > 0 && instData[0] is string enemyId)
        {
            Init(GameManager.Instance.EnemyDB.Get(enemyId)?.data);
        }
        else
        {
            // EnemyDB를 거치지 않고 소환된 경우 프리팹에 지정된 기본 데이터 사용
            Init(mData);
        }
        if(GameManager.Instance.IsMapDebug)
        {
            path = CreateTestPath();
        }
        else
        {
            int index = (int)instData[1];
            path = MapManager.Instance.GetPath(index).ToList();
        }
    }
    public void Init(IEnemySO data)
    {
        Data = data;
        if(Data != null) m_hp = MaxHP;
        UpdateHPBar();
        
        pathIndex = 0;
    }
    // 테스트용 경로: (-3,-3)에서 출발해 지그재그로 (3,3)에 도착
    private static List<Vector2> CreateTestPath()
    {
        List<Vector2> corners =
        new (){
            new Vector2(-3,-3),
            new Vector2( 3,-3),
            new Vector2( 3, 0),
            new Vector2(-3, 0),
            new Vector2(-3, 3),
            new Vector2( 3, 3),
        };
        return corners;
        /*
        List<ITileMap> result = new List<ITileMap>();
        result.Add(new FakeTile { Pos = corners[0], Type = TileType.Start });
        for(int i = 1; i < corners.Length; i++)
        {
            Vector2 from = corners[i - 1];
            Vector2 to = corners[i];
            int steps = Mathf.RoundToInt(Vector2.Distance(from, to));
            for(int s = 1; s <= steps; s++)
            {
                result.Add(new FakeTile { Pos = Vector2.Lerp(from, to, (float)s / steps), Type = TileType.Road });
            }
        }
        ((FakeTile)result[result.Count - 1]).Type = TileType.Goal;
        return result;
        */
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
            UpdateHPBar();
            if(m_hp <= 0)
            {
                SendDestroy();
            }
        }
    }

    public float MaxHP => Data.HP;
    private void UpdateHPBar()
    {
        if(hpBar == null || Data == null || MaxHP <= 0) return;
        hpBar.SetRatio(m_hp / MaxHP);
    }

    public IEnemySO Data {get;private set;}
    [Tooltip("EnemyDB 없이 소환될 때 사용할 기본 데이터")]
    [SerializeField]private EnemyData mData;
    [Tooltip("회전시킬 외형(스프라이트) Transform. HP바 등은 회전하지 않음")]
    [SerializeField]private Transform body;
    [SerializeField]private HPBar hpBar;

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
    private List<Vector2> path;
    private int pathIndex = 0;

    public void Update()
    {
        if(!PhotonNetwork.IsMasterClient) return;
        if(Data == null || path == null || pathIndex >= path.Count) return;

        float moveDist = Data.Speed * Time.deltaTime;
        Vector2 cur = Position;
        while(pathIndex < path.Count)
        {
            Vector2 target = path[pathIndex];
            float dist = Vector2.Distance(cur, target);
            if(dist > moveDist)
            {
                cur = Vector2.MoveTowards(cur, target, moveDist);
                break;
            }
            // 이번 프레임에 목표 타일 도달: 남은 이동량을 다음 타일로 이어서 사용
            cur = target;
            moveDist -= dist;
            pathIndex++;
        }
        Position = cur;

        // 프레임 이동량이 아닌 다음 목표 타일 방향을 바라봄 (프레임레이트 무관)
        if(pathIndex < path.Count)
        {
            Vector2 dir = path[pathIndex] - cur;
            if(dir != Vector2.zero)
            {
                if(body != null)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    body.eulerAngles = new Vector3(0, 0, angle);
                }
                else
                {
                    this.SetAngle(dir);
                }
            }
        }

        if(pathIndex >= path.Count)
        {
            OnReachGoal();
        }
    }
    // 마스터에서만 호출됨 (Update가 마스터 전용)
    private void OnReachGoal()
    {
        SendDestroy();
        PlayerHP.Damage(1);
    }
    /// <summary>
    /// 모든 클라이언트에 적 제거 패킷 전송 (마스터 전용)
    /// </summary>
    private void SendDestroy()
    {
        if(!PhotonNetwork.IsMasterClient) return;
        PacketData d = new PacketData();
        d.Write(Enemy.EnemyDestroy);
        d.Write(IndexId);
        IPacketer.SendPacket(MainGamePacketer.Instance.photonView,d);
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
        Debug.Log($"TakeDamage {dmgInfo.Dmg}");
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
    public static void EnemyCreateEvent(PacketData data)
    {
        string id = data;
        Vector2 pos = data;
        float angle = data;
        Quaternion q = Quaternion.Euler(new Vector3(0,0,angle));
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(id,pos,q);
        }
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