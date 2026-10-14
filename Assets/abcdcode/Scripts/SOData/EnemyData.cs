using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "SOData/EnemyData")]
public class EnemyData : SOData, IEnemySO
{
    [SerializeField]protected float hp = 10;
    [SerializeField]protected float speed = 2;
    public virtual float HP => hp;

    public virtual float Speed => speed;
}
