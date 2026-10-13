using UnityEngine;

public class Enemy : MultiMono, IEnemy, IdOwn
{
    
    public float HP { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public float MaxHP => throw new System.NotImplementedException();

    public IEnemySO Data => throw new System.NotImplementedException();

    public int IndexId { get; set;}

    public override void Delete()
    {
        base.Delete();
        GameManager.Instance.Container_Enemy.DeleteObj(this);
    }

    public void Init(System.Numerics.Vector2 pos)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(IDamageInfo dmgInfo)
    {
        throw new System.NotImplementedException();
    }
}