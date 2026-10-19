using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
public class GameManager : SingletonBehavior<GameManager>
{
    public override void Awake()
    {
        base.Awake();
        prefabPool.Register();
        if(m_enemyDB != null) prefabPool.Register(m_enemyDB.Prefabs);
        if(m_towerDB != null) prefabPool.Register(m_towerDB.Prefabs);
        if (isOffDebug)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
        }
        if(PhotonNetwork.InRoom) PlayerGold.Set(PlayerGold.StartGold);
        IsGameOver = false;
        PlayerHP.Init(m_startPlayerHP);
        Container_Enemy = new EnemyContainer();
        Container_Tower = new TowerContainer();
        Container_Bullet = new BulletContainer();
        StageManager = new StageManager();
        StageManager.Init(m_StageData);
        if(PhotonNetwork.IsMasterClient)
        {
            if(IsMapDebug)
            {
                FakeTile ft = new FakeTile
        {
            Pos = new Vector2(0, 0),
            Type = TileType.Build
        };
        CreateTower(ft, "FireTower1_Lv3", PhotonNetwork.LocalPlayer);
            }
        
        /*
        Enemy e = PhotonNetwork.Instantiate("TestEnemy", new Vector3(0, 0), Quaternion.identity).GetComponent<Enemy>();
        e.Position = new Vector2(-3, -3);
        */
        }
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void RegisterPackets()
    {
        MainGamePacketer.Register(GameOverPacket, GameOverEvent);
        MainGamePacketer.Register(GameClearPacket, GameClearEvent);
    }
    public void Update()
    {
        if(IsGameOver) return;
        StageManager?.Update(Time.deltaTime);
    }
    public static void GameOverEvent(PacketData data)
    {
        Instance.GameOver();
    }
    /// <summary>
    /// PlayerHP가 0이 되면 모든 클라이언트에서 호출됨
    /// </summary>
    public void GameOver()
    {
        if(IsGameOver) return;
        IsGameOver = true;
        Debug.Log("Game Over");
        GameEndPanelUI.Instance.OpenGameOver();
    }
    public static void GameClearEvent(PacketData data)
    {
        Instance.GameClear();
    }
    /// <summary>
    /// 마지막 웨이브의 적이 모두 제거되면 모든 클라이언트에서 호출됨
    /// </summary>
    public void GameClear()
    {
        if(IsGameOver) return;
        IsGameOver = true;
        Debug.Log("Game Clear");
        GameEndPanelUI.Instance.OpenGameClear();
    }
    public bool IsGameOver { get; private set; }
    [SerializeField] private int m_startPlayerHP = 10;
    public const string GameOverPacket = "GameOver";
    public const string GameClearPacket = "GameClear";
    public PhotonView View { get; private set; }
    [SerializeField] private bool isOffDebug;
    [SerializeField]public bool IsMapDebug;
    public IObjContainer<IEnemy> Container_Enemy { get; private set; }
    public IObjContainer<ITower> Container_Tower { get; private set; }
    public IObjContainer<Bullet> Container_Bullet { get; private set; }
    public StageManager StageManager { get; private set; }

    [SerializeField] private StageData m_StageData;
    [SerializeField] private EnemyDB m_enemyDB;
    public EnemyDB EnemyDB => m_enemyDB;
    [SerializeField] private TowerDB m_towerDB;
    public TowerDB TowerDB => m_towerDB;
    public Enemy CreateEnemy(ITileMap tile, string id) => m_enemyDB != null ? m_enemyDB.Create(tile, id) : null;
    public Tower CreateTower(ITileMap tile, string id, Player owner) => m_towerDB != null ? m_towerDB.Create(tile, id, owner) : null;

    [SerializeField] private TowerData m_tData;
    [SerializeField] private NetworkPrefabPool prefabPool;
    
    [SerializeField] private MainGamePacketer m_mgp;
    public MainGamePacketer MainGamePacketer
    {
        get
        {
            return m_mgp;
        }
    }
}