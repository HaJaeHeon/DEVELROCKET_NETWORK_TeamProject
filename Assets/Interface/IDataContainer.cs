using System.Collections.Generic;

public interface ISODataContainer<V> where V : ISOData
{
    public List<V> GetList();
    public V GetData(string id);
}