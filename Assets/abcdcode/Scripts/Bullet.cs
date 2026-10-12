using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : NultiMono
{
    public virtual void Init(Tower owner, float speed, IDamageInfo dmg)
    {
        Owner = owner;
        m_speed = speed;
        m_dmg = dmg;
    }
    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(!PhotonNetwork.IsMasterClient) return; //마스터 아니면 빠지기
        var e = collision.GetComponent<IEnemy>();
        if(e == null) return;
    }
    public virtual void Update()
    {
        this.MoveForward(m_speed);
    }
    private float m_speed;
    private IDamageInfo m_dmg;
    public Tower Owner{get;private set;}
}