using Photon.Pun;
using UnityEngine;

public class ExplodeBullet : Bullet
{
    public override void BulletHitAction(PacketData data)
    {
        int eid = data;
        if(PhotonNetwork.IsMasterClient)
        {
            var list = GameManager.Instance.Container_Enemy.GetList();
            var hit = list.FindEnemy(eid);
            Vector2 center = hit != null ? hit.transform.position : transform.position;
            foreach(var item in list)
            {
                var e = item as Enemy;
                if(e == null) continue;
                if(Vector2.Distance(center, e.transform.position) <= area)
                {
                    e.TakeDamage(m_dmg);
                }
            }
        }
        if(isOnce)
        {
            Delete();
        }
    }
    [SerializeField]public float area = 1f;
}
