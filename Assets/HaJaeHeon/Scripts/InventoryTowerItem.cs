using UnityEngine;

public class InventoryTowerItem
{
    [SerializeField] private string itemId;
    [SerializeField] private string towerId;
    [SerializeField] private int towerLevel;
    [SerializeField] private float towerPrice;

    public string ItemId
    {
        get
        {
            return itemId;
        }
    }

    public string TowerId
    {
        get
        {
            return towerId;
        }
    }

    public int TowerLevel
    {
        get
        {
            return towerLevel;
        }
    }

    public float TowerPrice
    {
        get
        {
            return towerPrice;
        }
    }

    public InventoryTowerItem(string itemId, string towerId, float towerPrice, int towerLevel = 1)
    {
        this.itemId = itemId;
        this.towerId = towerId;
        this.towerPrice = towerPrice;
        this.towerLevel = towerLevel;
    }
}
