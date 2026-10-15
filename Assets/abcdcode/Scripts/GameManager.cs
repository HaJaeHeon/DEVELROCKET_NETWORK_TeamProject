using Photon.Pun;
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
        Container_Enemy = new EnemyContainer();
        Container_Tower = new TowerContainer();
        Container_Bullet = new BulletContainer();
        StageManager = new StageManager();
        StageManager.Init(m_StageData);
        if(PhotonNetwork.IsMasterClient)
        {
        FakeTile ft = new FakeTile
        {
            Pos = new Vector2(0, 0),
            Type = TileType.Build
        };
        CreateTower(ft, "FireTower1");
        /*
        Enemy e = PhotonNetwork.Instantiate("TestEnemy", new Vector3(0, 0), Quaternion.identity).GetComponent<Enemy>();
        e.Position = new Vector2(-3, -3);
        */
        }
    }
    public void Update()
    {
        StageManager?.Update(Time.deltaTime);
    }
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
    public Tower CreateTower(ITileMap tile, string id) => m_towerDB != null ? m_towerDB.Create(tile, id) : null;

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