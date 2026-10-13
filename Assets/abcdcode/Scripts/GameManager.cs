using Photon.Pun;
using UnityEditor.Tilemaps;
using UnityEngine;
public class GameManager : SingletonBehavior<GameManager>
{
    public override void Awake()
    {
        base.Awake();
        prefabPool.Register();
        if (isOffDebug)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
        }
        Container_Enemy = new EnemyContainer();
        Container_Tower = new TowerContainer();
        Containder_Bullet = new BulletContainer();
        Tower t = PhotonNetwork.Instantiate("NormalTower", new Vector3(0, 0), Quaternion.identity).GetComponent<Tower>();
        FakeTile ft = new FakeTile
        {
            Pos = new Vector2(0, 0),
            Type = TileType.Build
        };
        t.Init(ft, m_tData);
        Container_Tower.AddObj(t);

        Enemy e = PhotonNetwork.Instantiate("TestEnemy", new Vector3(0, 0), Quaternion.identity).GetComponent<Enemy>();
        e.Position = new Vector2(10, 10);
        Container_Enemy.AddObj(e);
    }
    public PhotonView View { get; private set; }
    [SerializeField] private bool isOffDebug;
    public IObjContainer<IEnemy> Container_Enemy { get; private set; }
    public IObjContainer<ITower> Container_Tower { get; private set; }
    public IObjContainer<Bullet> Containder_Bullet { get; private set; }

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