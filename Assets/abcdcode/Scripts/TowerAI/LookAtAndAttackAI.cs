using System.Collections.Generic;
using UnityEngine;

public class LookAtAndAttackAI : TParameterBridge<AttackTowerData>
{
    public override void Init(Tower parent)
    {
        base.Init(parent);
        parent.RPCEvent[AI_Shoot] = Shoot;
    }
    public override void GameUpdate()
    {
        base.GameUpdate();
        cool -= Time.deltaTime;
        if(cool <= 0)
        {
            var p = new PacketData();
            p.Write(AI_Shoot);
            IPacketer.SendPacket(Parent.photonView,p,Photon.Pun.RpcTarget.AllViaServer);
            cool = TData.Cool;
            Debug.Log("Send ExCute");
        }
    }
    public void Shoot(PacketData data)
    {
        Debug.Log("Shoot!");
        var b = TData.Bullet;
        b.Position = Parent.Position;
        b.Init(Parent,5,new DamageInfo(1,""));
    }
    public float cool;
    public const string AI_Shoot = "AI_Shoot";
}