using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "SOData/EnemyData")]
public class EnemyData : SOData, IEnemySO
{
    [SerializeField]protected float hp = 10;
    [SerializeField]protected float speed = 2;
    [SerializeField]protected float gold = 1;
    [SerializeField]protected int dmg = 1;
    [SerializeField]protected Sprite icon;
    public virtual float HP => hp;

    public virtual float Speed => speed;
    public virtual float Gold => gold;
    public virtual int Dmg => dmg;
    public virtual Sprite GetIcon()
    {
        return icon;
    }
}
