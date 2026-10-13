using Photon.Pun;
using UnityEditor.Tilemaps;
using UnityEngine;
public class GameManager : SingletonBehavior<GameManager>
{
    public override void Awake()
    {
        base.Awake();
        if(isOffDebug)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
        }
        Container_Enemy = new EnemyContainer();
        Container_Tower = new TowerContainer();
        Tower t = Instantiate(m_tTower).GetComponent<Tower>();
        FakeTile ft = new FakeTile
        {
            Pos = new Vector2(0, 0),
            Type = TileType.Build
        };
        t.Init(ft,m_tData);
        Container_Tower.AddObj(t);

        Enemy e = Instantiate(m_tEnemy).GetComponent<Enemy>();
        e.Position = new Vector2(10,10);
        Container_Enemy.AddObj(e);
    }
    public PhotonView View{get;private set;}
    [SerializeField]private bool isOffDebug;
    public IObjContainer<IEnemy> Container_Enemy{get;private set;}
    public IObjContainer<ITower> Container_Tower{get;private set;}

    [SerializeField]private GameObject m_tTower;
    [SerializeField]private GameObject m_tEnemy;
    [SerializeField]private TowerData m_tData;
}