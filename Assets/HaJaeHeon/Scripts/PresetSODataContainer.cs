using UnityEngine;

public class PresetSODataContainer : MonoBehaviour
{
    public TowerData data;

    public string TowerItemId { get; private set; }

    public void SetItemId(string itemid)
    {
        TowerItemId = itemid;
    }
}
