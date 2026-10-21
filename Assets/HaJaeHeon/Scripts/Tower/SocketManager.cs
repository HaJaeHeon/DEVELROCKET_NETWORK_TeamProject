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

public partial class SocketManager : MonoBehaviourPunCallbacks
{
    public static SocketManager Instance { get; private set;  }

    public event Action<string, bool, string> TowerRecallResultReceived;

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
            GameManager.Instance.TowerDB == null || HasPendingGoldChange(requester.ActorNumber))
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
        if (!PhotonNetwork.InRoom || IsLocalOperationPending)
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
        var game = GameManager.Instance;

        if (game == null || game.Container_Tower == null)
            return;

        Tower target = null;

        foreach (ITower item in game.Container_Tower.GetList())
        {
            if (item is Tower tower && tower.IndexId == towerIndexId)
            {
                target = tower;
                break;
            }
        }

        if (target != null && !TryDestroyTowerOnMaster(target))
        {
            PlayerInventoryRegistry.Instance?.SuspendAuthority("합성 후 기존 타워 제거에 실패했습니다.");
        }
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
        Debug.Log($"[설치 항목 조회] 사용자={requester.ActorNumber}, 요청항목={towerItemId}, 조회항목={item.ItemId}, 타워={item.TowerId}, 단계={item.TowerLevel}");
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
                Debug.Log($"[타워 생성 전] 사용자={requester.ActorNumber}, 항목={item.ItemId}, 생성타워={spawnTowerId}, 등록정보={entry.data.Id}, 등록단계={entry.data.Level}");
                createdTower = GameManager.Instance.CreateTower(currentTile, spawnTowerId, requester);

                if (createdTower != null && createdTower.Data != null)
                {
                    Debug.Log($"[타워 생성 후] 사용자={requester.ActorNumber}, 항목={item.ItemId}, 객체={createdTower.photonView.ViewID}, 실제타워={createdTower.Data.Id}, 실제단계={createdTower.Data.Level}");
                }
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

        if (!PhotonNetwork.IsMasterClient || targetPlayer == null)
            return;

        if (pendingPurchases.TryGetValue(targetPlayer.ActorNumber, out var purchase) &&
            changedProps.TryGetValue(PurchaseReceiptKey, out object value) &&
            value is string receipt &&
            receipt == purchase.RequestId)
        {
            pendingPurchases.Remove(targetPlayer.ActorNumber);

            purchase.Completed = true;
            purchase.Success = true;

            SendPurchaseResult(purchase);
            return;
        }

        HandleSaleGoldReceipt(targetPlayer, changedProps);
    }


    // 방을 나간다면 구매 기록, 구매 대기 정리
    public override void OnLeftRoom()
    {
        transactionSessionStopped = true;
        ResetAllRequestState();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        transactionSessionStopped = true;
        ResetAllRequestState();

        Debug.LogWarning($"Photon 연결 종료 사유: {cause}");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        transactionSessionStopped = true;
        ResetAllRequestState();

        Debug.LogWarning("대표 참여자(Master Client)가 변경되어 거래를 중단합니다.");
    }

    public override void OnJoinedRoom()
    {
        ResetAllRequestState();
        transactionSessionStopped = false;
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
