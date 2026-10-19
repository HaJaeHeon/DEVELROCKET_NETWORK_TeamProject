using UnityEngine;
using UnityEngine.EventSystems;

public class ShopSocket : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        if (!eventData.pointerDrag.TryGetComponent(out InventoryDraggable draggable))
            return;

        if (TowerService.Instance == null)
            return;

        TowerService.Instance.RecallSell(draggable);
    }
}
