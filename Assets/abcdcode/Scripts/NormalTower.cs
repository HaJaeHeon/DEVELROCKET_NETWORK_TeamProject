using Photon.Pun;
using UnityEngine;

public class NormalTower : TowerMono,ITower
{
    public ITowerSO Data{get;private set;}
    public string Owner{get;private set;}
}