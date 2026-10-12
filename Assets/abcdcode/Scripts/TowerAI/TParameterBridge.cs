public class TParameterBridge<T> : TowerAI where T : TowerData
{
    protected T TData
    {
        get
        {
            return Parent.Data as T;
        }
    }
}