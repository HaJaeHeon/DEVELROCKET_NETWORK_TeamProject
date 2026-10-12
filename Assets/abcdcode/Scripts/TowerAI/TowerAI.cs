using System.Collections.Generic;

public abstract class TowerAI
{
    public virtual void Init(Tower parent)
    {
        Parent = parent;
    }
    public virtual void GameUpdate()
    {
        
    }

    public Tower Parent{get;set;}
}