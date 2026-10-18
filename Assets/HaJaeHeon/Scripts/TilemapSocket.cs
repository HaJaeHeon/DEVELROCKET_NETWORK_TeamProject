using System;
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

    private GameObject pendingInventoryTower;
    private bool isBuildRequestPending;

    public event Action<bool> TowerRecallResultReceived;

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

        GameObject inventoryButtonObject = inventoryTower.InventoryItemObject;

        if (inventoryButtonObject == null)
        {
            Debug.LogError("인벤토리 버튼 null");

            return;
        }

        TryAttatchImage(inventoryTower, inventoryButtonObject);
    }

    public void TryAttatchImage(InventoryDraggable inventoryTower, GameObject inventoryButtonObject)
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

        if (cellType != TileType.Build)
            return;

        if (isBuildRequestPending)
            return;

        pendingInventoryTower = inventoryButtonObject;
        isBuildRequestPending = true;

        RequestTowerSpawn(cell, cellType, towerId);
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
    private void RpcTowerSpawn(int x, int y, int typeValue, string towerId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Player requester = info.Sender;

        if (requester == null)
            return;

        Vector2Int cell = new Vector2Int(x, y);

        if (map == null || tilemap == null ||
            !tilemap.HasTile(new Vector3Int(x, y, 0)) ||
            map.GetTileType(cell) != TileType.Build ||
            (TileType)typeValue != TileType.Build)
        {
            photonView.RPC(nameof(RpcTowerSpawnResult), requester, false);
            return;
        }

        CurrentTile currentTile = new CurrentTile
        {
            pos = new Vector2(x, y) + towerSpawnTransformOffset,
            type = (TileType)typeValue
        };

        Tower createdTower =
            GameManager.Instance.CreateTower(currentTile, towerId, requester);

        bool success = createdTower != null;

        photonView.RPC(nameof(RpcTowerSpawnResult),
            requester,
            success);
    }

    private void RequestTowerSpawn(Vector2Int pos, TileType type, string towerId)
    {
        photonView.RPC(nameof(RpcTowerSpawn), RpcTarget.MasterClient, pos.x, pos.y, (int)type, towerId);
        Debug.Log($"{PhotonNetwork.LocalPlayer.ActorNumber}가 건설 요청");
    }

    public void RequestTowerRecall(int towerViewId)
    {
        if (!PhotonNetwork.InRoom || towerViewId <= 0)
            return;

        photonView.RPC(
            nameof(RpcRequestTowerRecall),
            RpcTarget.MasterClient,
            towerViewId);
    }

    [PunRPC]
    private void RpcRequestTowerRecall(int towerViewId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Player requester = info.Sender;

        if (requester == null)
            return;

        PhotonView towerView = PhotonView.Find(towerViewId);
        Tower tower = towerView != null
            ? towerView.GetComponent<Tower>()
            : null;

        bool success = tower != null &&
            tower.OwnerPlayer != null &&
            tower.OwnerPlayer.ActorNumber == requester.ActorNumber;

        if (success)
        {
            PacketData destroyPacket = new PacketData();
            destroyPacket.Write(Tower.TowerDestroy);
            destroyPacket.Write(tower.IndexId);

            IPacketer.SendPacket(
                GameManager.Instance.MainGamePacketer.photonView,
                destroyPacket);
        }

        photonView.RPC(
            nameof(RpcTowerRecallResult),
            requester,
            success);
    }

    [PunRPC]
    private void RpcTowerRecallResult(bool success)
    {
        TowerRecallResultReceived?.Invoke(success);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        pendingInventoryTower = null;
        isBuildRequestPending = false;
        Debug.LogWarning($"Photon 연결 종료 사유: {cause}");
    }

    [PunRPC]
    private void RpcTowerSpawnResult(bool success)
    {
        if (!isBuildRequestPending)
            return;

        if (success)
        {
            bool removed =
                PlayerInventory.Instance.TryRemoveTower(pendingInventoryTower);

            if (removed)
            {
                ButtonTowerPool.Instance.Return(pendingInventoryTower);
            }
            else
            {
                Debug.LogError("건설은 성공했지만 인벤토리 제거에 실패했습니다.");
            }
        }
        else
        {
            Debug.Log("타워 건설에 실패했습니다.");
        }

        pendingInventoryTower = null;
        isBuildRequestPending = false;
    }
}
