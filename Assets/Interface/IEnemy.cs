using System.Numerics;

public interface IEnemy : IMonoObj
{
    public void Init(Vector2 pos);
    public float HP{get;set;}
    public float MaxHP{get;}
    public IEnemySO Data{get;}
    public void TakeDamage(IDamageInfo dmgInfo);
    
}