using UnityEngine;
using UnityEngine.EventSystems;

public class TilemapSocket : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            Debug.Log("eventData null");
            return;
        }

        InventoryDraggable inventoryTower = eventData.pointerDrag.GetComponent<InventoryDraggable>();

        if (inventoryTower == null)
        {
            Debug.Log("image(draggableImage) null");
            return;
        }

        GameObject inventoryButtonObject = inventoryTower.InventoryItemObject;

        if (inventoryButtonObject == null)
        {
            Debug.LogError("인벤토리 버튼 null");

            return;
        }

        if (SocketManager.Instance == null)
            return;

        SocketManager.Instance.TryAttatchImage(inventoryTower, inventoryButtonObject);
    }
}
