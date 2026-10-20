using System.Collections.Generic;
using UnityEngine;

public class ShopTower : MonoBehaviour
{
    private readonly List<TowerData> towerList = new();

    public void Initialize()
    {
        towerList.Clear();

        foreach (TowerDB.Entry entry in GameManager.Instance.TowerDB.Map.Values)
        {
            if (entry == null || entry.data == null || entry.prefab == null)
                continue;

            if (entry.data.Level == 1 && entry.id == entry.data.Id)
                towerList.Add(entry.data);
        }
    }

    public bool HasCandidates => towerList.Count > 0;

    public TowerData GetRandomTower()
    {
        return towerList[Random.Range(0, towerList.Count)];
    }
}
