using System.Collections.Generic;

public interface ISODataContainer<T,V> : ISingleton<T> where V : ISOData
{
    public List<V> GetList();
    public V GetData(string id);
}