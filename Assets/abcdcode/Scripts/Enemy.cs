using UnityEngine;

public class Enemy : IEnemy
{
    public float HP { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public float MaxHP => throw new System.NotImplementedException();

    public IEnemySO Data => throw new System.NotImplementedException();

    public UnityEngine.Vector2 Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public UnityEngine.Vector2 Scale { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public GameObject Obj => throw new System.NotImplementedException();

    public void Delete()
    {
        throw new System.NotImplementedException();
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