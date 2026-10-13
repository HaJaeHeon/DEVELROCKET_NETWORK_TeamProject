using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
/// <summary>
/// 사거리 내에 적이 들어오면 인식하고, cool 주기마다 공격하는 패턴
/// </summary>
public class LookAtAndAttackAI : TParameterBridge<AttackTowerData>
{
    public override void Init(Tower parent)
    {
        base.Init(parent);
        parent.RPCEvent[AI_Shoot] = Shoot;
        parent.RPCEvent[AI_FoundTarget] = SetTarget;
        curTarget = -1;
        cool = TData.Cool;
    }
    public override void GameUpdate()
    {
        base.GameUpdate();
        if(PhotonNetwork.IsMasterClient)
        {
            if(TryFindTargetAndRotateAngle())
            {
                TryAttack();
            }
            else
            {
                curTarget = -1;
                var e = FindTarget();
                if(e != null)
                {
                    curTarget = (e as Enemy).IndexId;
                    var p = new PacketData();
                    p.Write(AI_FoundTarget);
                    p.Write(curTarget);
                    IPacketer.SendPacket(Parent.photonView,p,Photon.Pun.RpcTarget.AllViaServer);
                    return;
                }
            }
        }
        else
        {
            TryFindTargetAndRotateAngle();
        }
    }
    public void TryAttack()
    {
        cool -= Time.deltaTime;
        if(cool <= 0)
        {
            var p = new PacketData();
            p.Write(AI_Shoot);
            IPacketer.SendPacket(Parent.photonView,p,Photon.Pun.RpcTarget.AllViaServer);
            cool = TData.Cool;
        }
    }
    public void Shoot(PacketData data)
    {
        Debug.Log("Shoot!");
        var b = TData.Bullet;
        b.Position = Parent.Position;
        b.transform.eulerAngles = new Vector3(0,0,Parent.Angle);
        b.Init(Parent,5,new DamageInfo(1,""));
    }
    public void SetTarget(PacketData data)
    {
        curTarget = data;
    }
    public IEnemy FindTarget()
    {
        foreach(var e in GameManager.Instance.Container_Enemy.GetList())
        {
            float dist = CheckRange(e);
            if(dist <= TData.Range)
            {
                return e;
            }
        }
        return null;
    }
    public float CheckRange(IEnemy enemy)
    {
        var leng = enemy.Position - Parent.Position;
        var dist = Math.Sqrt(leng.x * leng.x + leng.y * leng.y);
        return (float)dist;
    }
    public bool TryFindTargetAndRotateAngle()
    {
        if(curTarget == -1) return false;
        var t = GameManager.Instance.Container_Enemy.GetList().FindEnemy(curTarget);
        if(t == null) return false;
        if(CheckRange(t) > TData.Range) return false;
        Parent.Angle = Parent.GetAngle(t.Position);
        return true;
    }
    private int curTarget;
    public float cool;
    public const string AI_Shoot = "AI_Shoot";
    public const string AI_FoundTarget = "AI_FoundTarget";
}