using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class CurrentTile: ITileMap
{
    public Vector2 pos { get; set; }
    public TileType type { get; set; }

    public TileType Type => type;

    Vector2 ITileMap.Pos => pos;
}

public class SocketManager : MonoBehaviourPunCallbacks
{
    public static SocketManager Instance { get; private set;  }

    public event Action<bool, string> TowerRecallResultReceived;

    public bool IsBuildRequestPending => isBuildRequestPending;

    [SerializeField] private MapManager map;
    [SerializeField] private Tilemap tilemap;

    [SerializeField] private Camera targetCamera;

    [SerializeField] private Vector2 towerSpawnTransformOffset = new Vector2(0.5f, 0.5f);

    [SerializeField] private BuyTower buyTower;

    private const string PurchaseReceiptKey = "TowerPurchaseReceipt";

    private sealed class PurchaseRecord
    {
        public Player Buyer;
        public string RequestId;
        public string ItemId;
        public bool Completed;
        public bool Success;
    }

    // 같은 요청을 다시 받아도 중복 구매하지 않도록 결과를 기억한다.
    private readonly Dictionary<(int, string), PurchaseRecord>
        purchaseRecords = new();

    // 재화 반영을 기다리는 동안 같은 사용자의 다음 구매를 제한한다.
    private readonly Dictionary<int, PurchaseRecord>
        pendingPurchases = new();

    private GameObject pendingInventoryTower;
    private bool isBuildRequestPending;

    private string pendingBuildItemId;


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;


        map = MapManager.Instance;
        targetCamera = Camera.main;
    }

    private void Start()
    {
        if (buyTower != null)
        {
            buyTower.PurchaseRequested += RequestTowerPurchase;
        }
        else
        {
            Debug.LogError("구매 요청을 받을 BuyTower가 연결되지 않았습니다.");
        }

        if (map == null)
        {
            Debug.LogError("맵이 준비되지 않음");
            return;
        }

        tilemap = map.Tilemap;
    }

    // 구매 요청
    private void RequestTowerPurchase(string requestId, string towerId)
    {
        if (!PhotonNetwork.InRoom || photonView.ViewID <= 0)
        {
            buyTower.HandlePurchaseResult(requestId, false, "");
            return;
        }

        photonView.RPC(nameof(RpcRequestTowerPurchase), RpcTarget.MasterClient, requestId, towerId);
    }

    [PunRPC]
    private void RpcRequestTowerPurchase(string requestId, string towerId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient || info.Sender == null)
            return;

        Player requester = info.Sender;

        if (string.IsNullOrEmpty(requestId) || string.IsNullOrEmpty(towerId))
            return;

        var key = (requester.ActorNumber, requestId);

        // 이미 받은 요청이라면 다시 결제하지 않는다.
        if (purchaseRecords.TryGetValue(key, out var previous))
        {
            if (previous.Completed)
                SendPurchaseResult(previous);

            return;
        }

        var record = new PurchaseRecord
        {
            Buyer = requester,
            RequestId = requestId
        };

        purchaseRecords.Add(key, record);

        var registry = PlayerInventoryRegistry.Instance;

        if (registry == null || !registry.CanManage || GameManager.Instance == null ||
            GameManager.Instance.TowerDB == null || pendingPurchases.ContainsKey(requester.ActorNumber))
        {
            RejectPurchase(record);
            return;
        }

        TowerDB.Entry entry = GameManager.Instance.TowerDB.Get(towerId);

        TowerData data = entry != null ? entry.data : null;

        if (data == null || entry.prefab == null || data.Id != towerId || float.IsNaN(data.Cost)
            || float.IsInfinity(data.Cost) || data.Cost < 0f)
        {
            RejectPurchase(record);
            return;
        }

        float currentGold = PlayerGold.Get(requester);

        if (float.IsNaN(currentGold) ||
            float.IsInfinity(currentGold) ||
            currentGold < data.Cost)
        {
            RejectPurchase(record);
            return;
        }

        if (!registry.TryAddItem(requester, data, out var item))
        {
            RejectPurchase(record);
            return;
        }

        record.ItemId = item.ItemId;

        // 속성 변경 요청 전에 대기 기록을 먼저 저장한다.
        pendingPurchases.Add(requester.ActorNumber, record);

        bool sent = requester.SetCustomProperties(
            new Hashtable
            {
            { PlayerGold.Key, currentGold - data.Cost },
            { PurchaseReceiptKey, requestId }
            });

        if (!sent)
        {
            pendingPurchases.Remove(requester.ActorNumber);
            registry.TryRollbackAddedItem(requester, item.ItemId);

            RejectPurchase(record);
        }
    }

    // 타워 이미지 붙이기
    public void TryAttatchImage(InventoryDraggable inventoryTower, GameObject inventoryButtonObject)
    {
        if (!PhotonNetwork.InRoom || isBuildRequestPending)
            return;

        if (inventoryTower == null || inventoryButtonObject == null ||
            PlayerInventory.Instance == null ||
            map == null || tilemap == null || targetCamera == null)
            return;

        if (!PlayerInventory.Instance.HasTower(inventoryButtonObject))
            return;

        PresetSODataContainer container =
            inventoryTower.GetComponent<PresetSODataContainer>();

        if (container == null || string.IsNullOrEmpty(container.TowerItemId))
        {
            Debug.LogWarning("설치할 항목의 보유 식별자가 없습니다.");
            return;
        }

        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int cell = (Vector2Int)tilemap.WorldToCell(worldPosition);

        if (!tilemap.HasTile(new Vector3Int(cell.x, cell.y, 0)))
            return;

        TileType cellType = map.GetTileType(cell);

        if (cellType != TileType.Build)
            return;

        pendingInventoryTower = inventoryButtonObject;
        pendingBuildItemId = container.TowerItemId;
        isBuildRequestPending = true;

        RequestTowerSpawn(cell, cellType, pendingBuildItemId);
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

    private void DespawnTower(int towerIndexId)
    {
        PacketData packet = new PacketData();
        packet.Write(Tower.TowerDestroy);
        packet.Write(towerIndexId);

        IPacketer.SendPacket(GameManager.Instance.MainGamePacketer.photonView, packet);
    }

    [PunRPC]
    private void RpcTowerSpawn(int x, int y, int typeValue, string towerItemId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient || info.Sender == null)
            return;

        Player requester = info.Sender;
        PlayerInventoryRegistry registry = PlayerInventoryRegistry.Instance;

        Vector2Int cell = new Vector2Int(x, y);

        if (registry == null || !registry.CanManage || GameManager.Instance == null || GameManager.Instance.TowerDB == null ||
            map == null || tilemap == null || !tilemap.HasTile(new Vector3Int(x, y, 0)) ||
            map.GetTileType(cell) != TileType.Build || (TileType)typeValue != TileType.Build)
        {
            photonView.RPC(nameof(RpcTowerSpawnResult), requester, towerItemId, false);
            return;
        }

        if (!registry.TryReserveItem(requester, towerItemId, PlayerInventoryRegistry.ItemOperation.Build,
            out string reservationId, out InventoryTowerItem item))
        {
            photonView.RPC(nameof(RpcTowerSpawnResult), requester, towerItemId, false);
            return;
        }
        Tower targetTower = null;
        TowerData nextData = null;

        if (TryFindTowerAtCell(cell, out ITower existingTower))
        {
            targetTower = existingTower as Tower;

            if (targetTower == null ||
                targetTower.OwnerPlayer == null ||
                targetTower.OwnerPlayer.ActorNumber != requester.ActorNumber ||
                targetTower.Data.Id != item.TowerId)
            {
                registry.TryCancelReservation(requester, towerItemId, reservationId);
                photonView.RPC(nameof(RpcTowerSpawnResult), requester, towerItemId, false);
                return;
            }

            nextData = targetTower.Data.NextLevel as TowerData;

            if (nextData == null)
            {
                registry.TryCancelReservation(requester, towerItemId, reservationId);
                photonView.RPC(nameof(RpcTowerSpawnResult), requester, towerItemId, false);
                return;
            }
        }

        string spawnTowerId = nextData != null ? nextData.Id : item.TowerId;

        Vector2 spawnPosition = targetTower != null
            ? targetTower.Position
            : new Vector2(x, y) + towerSpawnTransformOffset;

        int oldTowerIndexId = targetTower != null ? targetTower.IndexId : -1;

        Tower createdTower = null;
        bool success = false;

        try
        {
            TowerDB.Entry entry = GameManager.Instance.TowerDB.Get(spawnTowerId);

            if (entry != null &&
                entry.data != null &&
                entry.prefab != null &&
                entry.prefab.GetComponent<Tower>() != null)
            {
                CurrentTile currentTile = new CurrentTile
                {
                    pos = spawnPosition /*+ towerSpawnTransformOffset*/,
                    type = TileType.Build
                };

                createdTower = GameManager.Instance.CreateTower(currentTile, spawnTowerId, requester);

                if (createdTower != null)
                    success = registry.TryConsumeItem(requester, towerItemId, reservationId);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (!success)
            {
                registry.TryCancelReservation(requester, towerItemId, reservationId);

                if (createdTower != null)
                {
                    GameManager.Instance.Container_Tower.DeleteObj(createdTower);
                    PhotonNetwork.Destroy(createdTower.gameObject);
                }
            }
        }
        if (success && oldTowerIndexId >= 0)
            DespawnTower(oldTowerIndexId);

        photonView.RPC(nameof(RpcTowerSpawnResult), requester, towerItemId, success);
    }

    private void RequestTowerSpawn(Vector2Int pos, TileType type, string towerItemId)
    {
        photonView.RPC(nameof(RpcTowerSpawn), RpcTarget.MasterClient, pos.x, pos.y, (int)type, towerItemId);
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

    private bool TryFindTowerAtCell(Vector2Int cell, out ITower foundTower)
    {
        foundTower = null;

        if (tilemap == null || GameManager.Instance == null || GameManager.Instance.Container_Tower == null)
            return false;

        foreach (ITower tower in GameManager.Instance.Container_Tower.GetList())
        {
            if (tower == null || (tower is UnityEngine.Object unityObject && unityObject == null))
                continue;

            if (tower.CurTile != null)
                Debug.Log($"타일 : {tower.CurTile} / 타일 위치 : {tower.CurTile.Pos} / 타일 타입 : {tower.CurTile.Type}");

            Vector3 worldPosition = new Vector3(tower.Position.x, tower.Position.y, 0f);

            Vector3Int towerCell = tilemap.WorldToCell(worldPosition);

            if (towerCell.x != cell.x || towerCell.y != cell.y)
                continue;

            foundTower = tower;
            return true;
        }

        return false;
    }

    [PunRPC]
    private void RpcRequestTowerRecall(int towerViewId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient || info.Sender == null)
            return;

        Player requester = info.Sender;
        PlayerInventoryRegistry registry = PlayerInventoryRegistry.Instance;

        PhotonView towerView = PhotonView.Find(towerViewId);
        Tower tower = towerView != null ? towerView.GetComponent<Tower>() : null;

        if (registry == null || tower == null ||
            tower.OwnerPlayer == null ||
            tower.OwnerPlayer.ActorNumber != requester.ActorNumber ||
            !(tower.Data is TowerData towerData))
        {
            photonView.RPC(nameof(RpcTowerRecallResult), requester, false, "");
            return;
        }

        // 회수할 타워를 보유 기록에 등록하고 새 식별자를 발급한다.
        if (!registry.TryAddItem(requester, towerData, out var item))
        {
            photonView.RPC(nameof(RpcTowerRecallResult), requester, false, "");
            return;
        }

        bool success = false;

        try
        {
            PacketData destroyPacket = new PacketData();
            destroyPacket.Write(Tower.TowerDestroy);
            destroyPacket.Write(tower.IndexId);

            IPacketer.SendPacket(GameManager.Instance.MainGamePacketer.photonView, destroyPacket);

            success = true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            registry.TryRollbackAddedItem(requester, item.ItemId);
        }

        photonView.RPC(nameof(RpcTowerRecallResult), requester, success, success ? item.ItemId : "");
    }

    [PunRPC]
    private void RpcTowerRecallResult(bool success, string itemId, PhotonMessageInfo info)
    {
        if (info.Sender != PhotonNetwork.MasterClient)
            return;

        TowerRecallResultReceived?.Invoke(success, itemId);
    }


    [PunRPC]
    private void RpcTowerSpawnResult(string itemId, bool success, PhotonMessageInfo info)
    {
        if (info.Sender != PhotonNetwork.MasterClient)
            return;

        if (!isBuildRequestPending || pendingBuildItemId != itemId)
            return;

        if (!success)
        {
            Debug.Log("타워 설치가 거절되었습니다.");
            ClearBuildState();
            return;
        }

        StartCoroutine(FinishBuildUI());
    }

    private System.Collections.IEnumerator FinishBuildUI()
    {
        // 놓기 입력 뒤 끌기 종료가 원래 부모를 복구할 시간을 준다.
        yield return null;

        if (!isBuildRequestPending)
            yield break;

        if (pendingInventoryTower != null &&
            PlayerInventory.Instance != null &&
            ButtonTowerPool.Instance != null)
        {
            bool removed =
                PlayerInventory.Instance.TryRemoveTower(pendingInventoryTower);

            if (removed)
                ButtonTowerPool.Instance.Return(pendingInventoryTower);
            else
                Debug.LogError("설치는 성공했지만 보관함 항목을 제거하지 못했습니다.");
        }
        else
        {
            Debug.LogError("설치 결과를 화면에 반영할 참조가 없습니다.");
        }

        ClearBuildState();
    }

    private void ClearBuildState()
    {
        pendingInventoryTower = null;
        pendingBuildItemId = null;
        isBuildRequestPending = false;
    }

    private void RejectPurchase(PurchaseRecord record)
    {
        record.Completed = true;
        record.Success = false;

        SendPurchaseResult(record);
    }

    private void SendPurchaseResult(PurchaseRecord record)
    {
        photonView.RPC(nameof(RpcTowerPurchaseResult), record.Buyer, record.RequestId, record.Success,
            record.Success ?
            record.ItemId : "");
    }

    [PunRPC]
    private void RpcTowerPurchaseResult(string requestId, bool success, string itemId, PhotonMessageInfo info)
    {
        // 대표 참여자가 보낸 결과만 받는다.
        if (info.Sender != PhotonNetwork.MasterClient)
            return;

        if (buyTower != null)
        {
            buyTower.HandlePurchaseResult(requestId, success, itemId);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!pendingPurchases.TryGetValue(targetPlayer.ActorNumber, out var record))
            return;

        if (!changedProps.TryGetValue(PurchaseReceiptKey, out object value))
            return;

        if (!(value is string receipt) || receipt != record.RequestId)
            return;

        pendingPurchases.Remove(targetPlayer.ActorNumber);

        record.Completed = true;
        record.Success = true;

        SendPurchaseResult(record);
    }


    // 방을 나간다면 구매 기록, 구매 대기 정리
    public override void OnLeftRoom()
    {
        purchaseRecords.Clear();
        pendingPurchases.Clear();

        ClearBuildState();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        purchaseRecords.Clear();
        pendingPurchases.Clear();

        ClearBuildState();

        Debug.LogWarning($"Photon 연결 종료 사유: {cause}");
    }

    // 파괴시 이벤트 구독 해제
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (buyTower != null)
            buyTower.PurchaseRequested -= RequestTowerPurchase;
    }
}
