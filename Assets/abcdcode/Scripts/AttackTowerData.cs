using UnityEngine;
public class AttackTowerData : TowerData
{
    protected float Range;
    protected float Damage;

    public override TowerType Type => TowerType.Attack;
}