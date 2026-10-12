using UnityEngine;
public class AttackTowerData : TowerData
{
    [SerializeField]protected float range;
    [SerializeField]protected float damage;
    [SerializeField]protected float cool;
    [SerializeField]protected GameObject bulletPrefab;
    public float Range => range;
    public float Damage => damage;
    public float Cool => cool;
    public virtual Bullet Bullet => Instantiate(bulletPrefab).GetComponent<Bullet>();

    public override TowerType Type => TowerType.Attack;
}