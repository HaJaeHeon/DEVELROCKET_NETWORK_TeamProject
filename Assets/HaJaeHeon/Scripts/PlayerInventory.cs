using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : SingletonBehavior<PlayerInventory>
{
    public const int MAXINVENTORYCOUNT = 5;
    private readonly List<GameObject> towerInventory = new();

    public override void Awake()
    {
        base.Awake();
    }



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
        if (tower == null)
        {
            Debug.LogError("추가할 인벤토리 타워가 없습니다.");
            return false;
        }

        // 인벤토리 상한선 보다 많이 구매하려고 한다면 false;
        if (towerInventory.Count >= MAXINVENTORYCOUNT)
        {
            Debug.LogError("인벤토리 상한보다 더 인벤토리에 추가하려고 함");
            return false;
        }
        PresetSODataContainer so = tower.GetComponentInChildren<PresetSODataContainer>();

        // so data가 없다면 false;
        if (so == null || so.data == null)
        {
            Debug.LogError("컴포넌트나 so데이터가 없음.");
            return false;
        }
        towerInventory.Add(tower);

        Debug.Log("인벤토리 추가 완료");
        return true;
    }

    // 인벤토리에서 타워 제거
    public bool TryRemoveTower(GameObject tower)
    {
        if (tower == null)
            return false;

        bool removed = towerInventory.Remove(tower);

        if (!removed)
            Debug.LogError("인벤토리에 해당 타워가 없습니다.");

        return removed;
    }
}
