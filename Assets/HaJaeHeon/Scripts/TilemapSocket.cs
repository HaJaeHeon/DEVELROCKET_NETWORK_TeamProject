using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class CurrentTile: ITileMap
{
    public Vector2 pos { get; set; }
    public TileType type { get; set; }

    public TileType Type => type;

    Vector2 ITileMap.Pos => pos;
}

public class TilemapSocket : MonoBehaviourPunCallbacks, IDropHandler
{
    [SerializeField] private MapManager map;
    [SerializeField] private Tilemap tilemap;

    [SerializeField] private Camera targetCamera;

    [SerializeField] private Vector2 towerSpawnTransformOffset = new Vector2(0.5f, 0.5f);

    private void Awake()
    {
        map = MapManager.Instance;
        targetCamera = Camera.main;
    }
    private void Start()
    {
        tilemap = map.Tilemap;
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


        string towerId = "";
        //string towerId = image.GetComponent<PresetSODataContainer>().data.Id;
        if(image.TryGetComponent<PresetSODataContainer>(out var component))
        {
            towerId = component.data.Id;
        }
        else
        {
            Debug.LogError("PresetSODataContainer is null");
        }

        if(cellType == TileType.Build)
        {
            RequestTowerSpawn(cell, cellType, towerId, PhotonNetwork.LocalPlayer);
        }
        else
        {
            Debug.Log("Is not BuildTile");
            return;
        }

    }

    private void SpawnTower(DraggableImage image, Vector2Int cell)
    {
        Sprite imageSprite = image.GetComponent<Image>().sprite;

        GameObject target = new GameObject("new_Image");

        SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
        renderer.sprite = imageSprite;
        renderer.sortingOrder = 10;

        target.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
    }

    [PunRPC]
    private void RpcTowerSpawn(int x, int y, int typeValue, string towerId, int actorNumber)
    {
        // 여기서
        // 건설 가능 구역 확인은 되었는데
        // 타워가 있는지 판단하고 
        // 만약 타워가 있다면 현재 내가 소환하려는 타워랑 같은지, 내 소유 타워인지 확인하고
        // 만약 타워가 같은 타워면 업그레이드
        // 되도록 하는 로직 필요 
        if (!PhotonNetwork.IsMasterClient)
            return;

        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        if (player == null)
            return;

        CurrentTile currentTile = new CurrentTile
        {
            pos = new Vector2(x,y) + towerSpawnTransformOffset,
            type = (TileType)typeValue
        };
        GameManager.Instance.CreateTower(currentTile, towerId, player);

        Debug.Log($"{actorNumber} 의 소유로 건설함");
    }

    private void RequestTowerSpawn(Vector2Int pos, TileType type, string towerId, Player player)
    {
        photonView.RPC(nameof(RpcTowerSpawn), RpcTarget.MasterClient, pos.x, pos.y, (int)type, towerId, player.ActorNumber);
        Debug.Log($"{player.ActorNumber}가 건설 요청");
    }
}
