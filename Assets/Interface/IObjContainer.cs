using System.Collections.Generic;

public interface IObjContainer<V> where V : IMonoObj
{
    public List<V> GetList();
    public void AddObj(V obj);
}