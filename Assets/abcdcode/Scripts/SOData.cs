using UnityEngine;

public class SOData : ScriptableObject, ISOData
{
    public virtual void SODataInit()
    {
        
    }
    [SerializeField]private string id;
    public string Id => id;
}