using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public const int MAXINVENTORYCOUNT = 5;
    private readonly List<GameObject> towerInventory = new();

    public bool IsFullInventory
    {
        get
        {
            return towerInventory.Count >= MAXINVENTORYCOUNT;
        }
    }

    public bool HasTower(string towerId)
    {
        return towerInventory.Exists(inventoryTower =>
        {
            if (inventoryTower == null)
                return false;

            PresetSODataContainer so = inventoryTower.GetComponentInChildren<PresetSODataContainer>();

            return so != null && 
                so.data != null && 
                so.data.Id == towerId;
        });
    }

    // 인벤토리에서 타워 추가 (같은 타워도 추가 허용)
    public bool TryAddTower(GameObject tower)
    {
        // 인벤토리 상한선 보다 많이 구매하려고 한다면 false;
        if (towerInventory.Count >= MAXINVENTORYCOUNT)
        {
            Debug.LogError("인벤토리 상한보다 더 인벤토리에 추가하려고 함");
            return false;
        }
        PresetSODataContainer so = tower.GetComponentInChildren<PresetSODataContainer>();

        // so data가 없다면 false; 
        if (so.data == null)
        {
            Debug.LogError("컴포넌트나 so데이터가 없음.");
            return false;
        }
        towerInventory.Add(tower);

        Debug.Log("인벤토리 추가 완료");
        return true;
    }

    // 인벤토리에서 타워 제거
    public bool TryRemoveTower(string towerid, int towerIndex)
    {
        PresetSODataContainer so = towerInventory[towerIndex].GetComponentInChildren<PresetSODataContainer>();

        if (so.data.Id != towerid)
        {
            Debug.LogError($"{towerIndex}의 위치에 있는 id가 {towerid}가 아님");
            return false;
        }
        ButtonTowerPool.Instance.Return(towerInventory[towerIndex]);
        towerInventory.RemoveAt(towerIndex);

        Debug.Log($"{towerIndex}위치의 타워 제거");

        return true;
    }
}
