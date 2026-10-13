using System.Collections.Generic;
using System.Linq;

public class EnemyContainer :  IObjContainer<IEnemy>
{
    public void AddObj(IEnemy obj)
    {
        items.Add(obj);
    }

    public void DeleteObj(IEnemy obj)
    {
        items.Remove(obj);
    }

    public List<IEnemy> GetList()
    {
        return items.ToList();
    }
    private List<IEnemy> items;
}