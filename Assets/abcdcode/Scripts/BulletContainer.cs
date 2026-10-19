using System.Collections.Generic;
using System.Linq;

public class BulletContainer : IObjContainer<Bullet>
{
    public void AddObj(Bullet obj)
    {
        if(items.Contains(obj))return;
        items.Add(obj);
    }

    public void DeleteObj(Bullet obj)
    {
        items.Remove(obj);
    }

    public List<Bullet> GetList()
    {
        return items.ToList();
    }
    private List<Bullet> items = new List<Bullet>();
}