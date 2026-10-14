using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : NultiMono, IdOwn
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void RegisterPackets()
    {
        MainGamePacketer.Register(BulletHit, BulletHitExCute);
    }
    public virtual void Init(Tower owner, float speed, IDamageInfo dmg)
    {
        Owner = owner;
        m_speed = speed;
        m_dmg = dmg;
        GameManager.Instance.Containder_Bullet.AddObj(this);
        this.SetIndex();
    }
    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hit");
        if(!PhotonNetwork.IsMasterClient) return; //마스터 아니면 빠지기
        if(isHit) return;
        var e = collision.GetComponent<IEnemy>();
        if(e == null) return;
        var re = e as Enemy;
        PacketData d = new ();
        d.Write(BulletHit);
        d.Write(IndexId);
        d.Write(re.IndexId);
        IPacketer.SendPacket(GameManager.Instance.MainGamePacketer.photonView,d);
        isHit = true;
    }
    public virtual void Update()
    {
        this.MoveForward(m_speed);
    }
    public override void Delete()
    {
        base.Delete();
        GameManager.Instance.Containder_Bullet.DeleteObj(this);
    }
    public static void BulletHitExCute(PacketData data)
    {
        int bid = data;
        int eid = data;
        var b = GameManager.Instance.Containder_Bullet.GetList().FindObj(bid);
        if(b != null)
        {
            b.Delete();
        }
        var e = GameManager.Instance.Container_Enemy.GetList().FindEnemy(eid);
    }
    private bool isHit;
    private float m_speed;
    private IDamageInfo m_dmg;
    public Tower Owner{get;private set;}
    public int IndexId {get; set;}

    public const string BulletHit = "BulletHit";
}