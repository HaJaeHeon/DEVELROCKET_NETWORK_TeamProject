using UnityEngine;

public class NultiMono : MonoBehaviour, IMonoObj
{
    public Vector2 Position
    {
        get
        {
            return this.transform.position;
        }
        set
        {
            this.transform.position = value;
        }
    }
    public Vector2 Scale
    {
        get
        {
            return this.transform.localScale;
        }
        set
        {
            transform.localScale = value;
        }
    }

    public GameObject Obj => this.gameObject;

    public virtual void Delete()
    {
        Destroy(this.gameObject);
    }
}