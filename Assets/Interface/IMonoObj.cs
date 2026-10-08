using UnityEngine;

public interface  IMonoObj
{
    public Vector2 Position{get;set;}
    public Vector2 Scale{get;set;}
    public GameObject Obj{get;}
    public void Delete();
}