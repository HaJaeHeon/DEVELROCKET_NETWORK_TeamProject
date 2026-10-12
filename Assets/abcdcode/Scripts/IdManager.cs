public class IdManager : Singleton<IdManager>
{
    public void Reset()
    {
        id = 0;
    }
    public int Next()
    {
        return id++;
    }
    public int id;
}