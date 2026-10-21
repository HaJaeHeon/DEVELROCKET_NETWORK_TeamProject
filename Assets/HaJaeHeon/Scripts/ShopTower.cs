using System.Collections.Generic;
using UnityEngine;

public class ShopTower : MonoBehaviour
{
    private readonly List<TowerData> towerList = new();

    private StageManager stageManager;

    private void OnEnable()
    {
        stageManager.OnWaveEnd += RerollShop;
    }
    private void OnDisable()
    {
        
    }

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

    private void RerollShop(int waveNum)
    {
        towerList.Clear();

        if (waveNum > 3)
            waveNum = 3;

        foreach (TowerDB.Entry entry in GameManager.Instance.TowerDB.Map.Values)
        {
            if (entry == null || entry.data == null || entry.prefab == null)
                continue;

            if (entry.data.Level == waveNum && entry.id == entry.data.Id)
                towerList.Add(entry.data);
        }

        Debug.Log("상점 리롤");
    }
}
