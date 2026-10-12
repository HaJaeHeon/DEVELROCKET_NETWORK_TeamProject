using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;

public class TestContainer : Singleton<TestContainer>, IObjContainer<IEnemy>
{
    public void AddObj(IEnemy obj)
    {
        
        throw new System.NotImplementedException();
    }

    public List<IEnemy> GetList()
    {
        throw new System.NotImplementedException();
    }

    public void DeleteObj(IEnemy obj)
    {
        throw new System.NotImplementedException();
    }

    private List<IEnemy> enemy;
}