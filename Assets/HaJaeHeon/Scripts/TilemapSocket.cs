using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilemapSocket : MonoBehaviour, IDropHandler
{
    [SerializeField] private MapManager map;
    [SerializeField] private Tilemap tilemap;

    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        map = MapManager.Instance;
        targetCamera = Camera.main;
    }
    private void Start()
    {
        tilemap = map.GetTileMap();
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            Debug.Log("eventData null");
            return;
        }
        DraggableImage image = eventData.pointerDrag.GetComponent<DraggableImage>();

        if (image == null)
        {
            Debug.Log("image(draggableImage) null");
            return;
        }

        TryAttatchImage(image);


    }

    public void TryAttatchImage(DraggableImage image)
    {
        if (map == null || targetCamera == null)
            return;

        if (tilemap == null)
            return;

        if (image == null)
            return;

        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = 0f;

        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);

        Vector2Int cell = (Vector2Int)tilemap.WorldToCell(worldPosition);

        if (!tilemap.HasTile(new Vector3Int(cell.x, cell.y, 0)))
            return;

        TileType cellType = map.GetTileType(cell);

        Debug.Log($"position2D : {cell}\n" +
            $"cellType : {cellType}");

        if(cellType == TileType.Build)
        {
            Sprite imageSprite = image.GetComponent<Image>().sprite;

            GameObject target = new GameObject("new_Image");

            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = imageSprite;
            renderer.sortingOrder = 10;

            target.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));

        }
        else
        {
            Debug.Log("Is not BuildTile");
            return;
        }

    }
}
