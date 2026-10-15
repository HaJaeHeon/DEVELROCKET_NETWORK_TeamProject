using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TilemapSocket : MonoBehaviour, IDropHandler
{
    private MapManager map;
    private Tilemap tilemap;

    private Sprite targetSprite;


    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        DraggableImage image = eventData.pointerDrag.GetComponent<DraggableImage>();

        if (image == null)
        {
            Debug.Log("image(draggableImage) null");
            return;
        }



    }

    public void TryAttatchImage()
    {
        if()
    }
}
