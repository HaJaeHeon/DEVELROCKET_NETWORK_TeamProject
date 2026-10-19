using System.Collections.Generic;
using UnityEngine;

public static class ExtensionUtil
{
    public static void SetIndex(this IdOwn owner)
    {
        owner.IndexId = IdManager.Instance.Next();
    }
    public static Enemy FindEnemy(this List<IEnemy> list, int id)
    {
        return list.Find(x=>(x as Enemy).IndexId == id) as Enemy;
    }
    public static Tower FindTower(this List<ITower> list, int id)
    {
        return list.Find(x=>(x as Tower).IndexId == id) as Tower;
    }
    public static T FindObj<T>(this List<T> list, int id) where T : IdOwn
    {
        return list.Find(x=>x.IndexId == id);
    }
    public static Color GetColor(MyColor mColor)
    {
        switch (mColor)
        {
            case MyColor.Red:
                return Color.red;

            case MyColor.Green:
                return Color.green;

            case MyColor.Blue:
                return Color.blue;

            default:
                return Color.white;
        }
    }
}