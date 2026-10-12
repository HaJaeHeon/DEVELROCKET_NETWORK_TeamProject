using System.Collections.Generic;

public interface IObjContainer<T,V> : ISingleton<T> where V : IMonoObj
{
    public List<V> GetList();
    public V AddObj();
}