using System;
using System.Reflection;
using UnityEngine;

public abstract class TowerData : SOData, ITowerSO
{
    [SerializeField]protected float cost;
    [SerializeField]protected string towerName;
    [SerializeField]protected string desc;
    [SerializeField]protected string AIScript;
    [SerializeField]protected Sprite icon;
    public virtual float Cost => cost;

    public virtual string Name => towerName;

    public virtual string Desc => desc;

    public virtual TowerType Type => TowerType.Special;
    public virtual TowerAI CreateAI()
    {
        var t = System.Type.GetType(AIScript);
        return (TowerAI)Activator.CreateInstance(t);
    }
    public virtual string GetDetail()
    {
        return Desc;
    }
    public virtual Sprite GetIcon()
    {
        return icon;
    }
}
