using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
        if(map == null)
        {
            Debug.LogError("맵이 준비되지 않음");
            return;
        }

        tilemap = map.Tilemap;
    }


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

        TryAttatchImage(inventoryTower);


    }

    public void TryAttatchImage(InventoryDraggable inventoryTower)
    {
        if (map == null || targetCamera == null)
            return;

        if (tilemap == null)
            return;

        if (inventoryTower == null)
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
        if(inventoryTower.TryGetComponent<PresetSODataContainer>(out var component) )
        {
            if(component.data == null)
            {
                Debug.LogError("컴포넌트의 데이터 비었음");
                return;
            }
            towerId = component.data.Id;
        }
        else
        {
            Debug.LogError("PresetSODataContainer is null");
            return;
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

        // 현재 타일에 타워 있는지 검사하고 타워 있으면 같은 타워인지, 내가 지은 타워인지 확인하고
        // 여기까지도 true면 타워 업그레이드??

        CurrentTile currentTile = new CurrentTile
        {
            pos = new Vector2(x,y) + towerSpawnTransformOffset,
            type = (TileType)typeValue
        };
        if(!GameManager.Instance.CreateTower(currentTile, towerId, player))
        {
            Debug.Log("건설 실패");
            return;
        }

        Debug.Log($"{actorNumber} 의 요청으로 건설함");
    }

    private void RequestTowerSpawn(Vector2Int pos, TileType type, string towerId, Player player)
    {
        photonView.RPC(nameof(RpcTowerSpawn), RpcTarget.MasterClient, pos.x, pos.y, (int)type, towerId, player.ActorNumber);
        Debug.Log($"{player.ActorNumber}가 건설 요청");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Photon 연결 종료 사유: {cause}");
    }
}
