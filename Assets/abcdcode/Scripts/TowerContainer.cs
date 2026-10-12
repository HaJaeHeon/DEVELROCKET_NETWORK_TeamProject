using System.Collections.Generic;
using System.Linq;

public class TowerContainer : SingletonBehavior<TowerContainer>, IObjContainer<ITower>
{
    public void AddObj(ITower obj)
    {
        towers.Add(obj);
    }

    public void DeleteObj(ITower obj)
    {
        towers.Remove(obj);
    }

    public List<ITower> GetList()
    {
        return towers.ToList();
    }
    private List<ITower> towers;
}