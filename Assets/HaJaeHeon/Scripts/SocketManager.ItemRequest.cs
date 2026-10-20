using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public partial class SocketManager
{
    public const float REFUNDRATE = 0.5f;

    public event Action<string, bool> TowerSellResultReceived;

    public bool IsPurchasePending => IsPurchasePending;

    private const string SellReceiptKey = "TowerSellReceipt";

    private sealed class ItemRequestRecord
    {
        public Player Requester;
        public string RequestId;
        public string TargetId;
        public bool IsRecall;

        public bool Completed;
        public bool Success;
        public string ItemId;
    }

    private readonly Dictionary<(int, string, bool), ItemRequestRecord> itemRequests = new();
    private readonly Dictionary<int, ItemRequestRecord> pendingSales = new();

    private bool transactionSessionStopped;

    // 거래 중단 상태 / 설치 결과 대기 / 구매 결과 대기 / 판매,회수 결과 대기
    public bool IsLocalOperationPending =>
        transactionSessionStopped ||
        isBuildRequestPending ||
        (buyTower != null && buyTower.IsPurchasePending) ||
        (TowerService.Instance != null && TowerService.Instance.IsBusy);

    // 사용자별 구매,판매, 대기 기록 확인(같은 사용자의 구매,판매,재화 변경이 겹치지 않게)
    private bool HasPendingGoldChange(int actorNumber)
    {
        return pendingPurchases.ContainsKey(actorNumber) || pendingSales.ContainsKey(actorNumber);
    }

    private static bool IsValidAmount(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
    }

    public void RequestTowerSell(string requestId, string itemId)
    {
        RequestItemOperation(false, requestId, itemId);
    }

    public void RequestTowerRecall(string requestId, int towerViewId)
    {
        if (towerViewId <= 0)
        {
            TowerRecallResultReceived?.Invoke(requestId, false, "");
            return;
        }

        RequestItemOperation(true, requestId, towerViewId.ToString());
    }

    private void RequestItemOperation(bool isRecall, string requestId, string targetId)
    {
        if (!PhotonNetwork.InRoom || transactionSessionStopped || photonView.ViewID <= 0 || string.IsNullOrEmpty(requestId) || string.IsNullOrEmpty(targetId))
        {
            DeliverItemResult(isRecall, requestId, false, "");
            return;
        }

        photonView.RPC(nameof(RpcRequestItemOperation), RpcTarget.MasterClient, isRecall, requestId, targetId);
    }

    // 사용자별 번호,요청 식별자,작업종류
    [PunRPC]
    private void RpcRequestItemOperation(bool isRecall, string requestId, string targetId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient || info.Sender == null || string.IsNullOrEmpty(requestId) || string.IsNullOrEmpty(targetId))
            return;

        Player requester = info.Sender;
        var key = (requester.ActorNumber, requestId, isRecall);

        if (itemRequests.TryGetValue(key, out var previous))
        {
            // 같은 요청 식별자로 다른 대상을 지정하면 처리하지 않는다.
            if (previous.TargetId != targetId)
                return;

            if (previous.Completed)
                SendItemResult(previous);

            return;
        }

        var record = new ItemRequestRecord
        {
            Requester = requester,
            RequestId = requestId,
            TargetId = targetId,
            IsRecall = isRecall
        };

        itemRequests.Add(key, record);

        var registry = PlayerInventoryRegistry.Instance;

        if (transactionSessionStopped || registry == null || !registry.CanManage)
        {
            CompleteItemRequest(record, false, "");
            return;
        }

        if (isRecall)
        {
            bool success = TryRecallTower(requester, targetId, out string itemId);
            CompleteItemRequest(record, success, itemId);
        }
        else
        {
            BeginTowerSale(record);
        }
    }

    // 판매 : inventory 확인 > 재화 지급
    // 해당 사용자의 다른 재화 변경 대기중?
    // >> 인벤토리 내 정보 예약(중복사용x)
    // >> 타워 등록 정보에서 가격을 읽어 판매 금액 책정
    // >> 가격,현재 재화,계산 결과 정상값인지 검사
    // >> inventory Registry에서 제거
    // >> 재화와 판매 확인 식별자 전송
    private void BeginTowerSale(ItemRequestRecord record)
    {
        Player requester = record.Requester;
        var registry = PlayerInventoryRegistry.Instance;
        var game = GameManager.Instance;

        if (game == null || game.TowerDB == null || HasPendingGoldChange(requester.ActorNumber))
        {
            CompleteItemRequest(record, false, "");
            return;
        }

        if (!registry.TryReserveItem(requester, record.TargetId, PlayerInventoryRegistry.ItemOperation.Sell, out string reservationId, out InventoryTowerItem item))
        {
            CompleteItemRequest(record, false, "");
            return;
        }

        var entry = game.TowerDB.Get(item.TowerId);
        TowerData data = entry != null ? entry.data : null;

        float currentGold = PlayerGold.Get(requester);
        float refund = data != null ? data.Cost * REFUNDRATE : -1f;
        float resultGold = currentGold + refund;

        if (data == null || data.Id != item.TowerId || !IsValidAmount(data.Cost) || !IsValidAmount(currentGold) || !IsValidAmount(refund) || !IsValidAmount(resultGold))
        {
            registry.TryCancelReservation(requester, item.ItemId, reservationId);
            CompleteItemRequest(record, false, "");
            return;
        }

        // 재화 변경 요청 전에 보유 기록에서 먼저 제거한다.
        if (!registry.TryConsumeItem(requester, item.ItemId, reservationId))
        {
            registry.TryCancelReservation(requester, item.ItemId, reservationId);
            CompleteItemRequest(record, false, "");
            return;
        }

        // 오프라인 방식(OfflineMode)의 즉시 응답에도 대응하도록 먼저 등록한다.
        pendingSales.Add(requester.ActorNumber, record);

        var properties = new Hashtable
        {
            { PlayerGold.Key, resultGold },
            { SellReceiptKey, record.RequestId }
        };

        bool sent = requester.SetCustomProperties(properties);

        if (!sent)
        {
            pendingSales.Remove(requester.ActorNumber);

            if (!registry.TryRestoreConsumedItem(requester, item))
            {
                // 재화 변경 요청을 전송하지 못하면 제거한 항목을 같은 항목 식별자로 복구
                // 복구까지 실패하면 아래 함수로 보유 기록 관리 권한을 중단 > 원인 출력
                registry.SuspendAuthority("판매 전송 실패 후 항목 복구에 실패했습니다.");
            }
            // 재화 변경이 확인되면 판매 확정시킴
            CompleteItemRequest(record, false, "");
        }
    }

    private void HandleSaleGoldReceipt(Player targetPlayer, Hashtable changedProps)
    {
        if (!pendingSales.TryGetValue(targetPlayer.ActorNumber, out var record))
            return;

        if (!changedProps.TryGetValue(SellReceiptKey, out object value) || !(value is string receipt) || receipt != record.RequestId)
            return;

        pendingSales.Remove(targetPlayer.ActorNumber);

        // 보유 기록 제거는 끝났으므로 결과만 확정한다.
        CompleteItemRequest(record, true, "");
    }

    private bool TryRecallTower(Player requester, string targetId, out string itemId)
    {
        itemId = "";

        if (!int.TryParse(targetId, out int viewId))
            return false;

        var registry = PlayerInventoryRegistry.Instance;
        var view = PhotonView.Find(viewId);
        var tower = view != null ? view.GetComponent<Tower>() : null;
        var game = GameManager.Instance;

        if (game == null || game.Container_Tower == null || tower == null || !tower.gameObject.activeInHierarchy || tower.OwnerPlayer == null || tower.OwnerPlayer.ActorNumber != requester.ActorNumber || !(tower.Data is TowerData data))
            return false;

        if (!registry.TryAddItem(requester, data, out var item))
            return false;

        if (!TryDestroyTowerOnMaster(tower))
        {
            if (!registry.TryRollbackAddedItem(requester, item.ItemId))
            {
                registry.SuspendAuthority("회수 실패 후 보유 기록 복구에 실패했습니다.");
            }

            return false;
        }

        itemId = item.ItemId;
        return true;
    }

    // 타워 제ㅐ거와 각 참여자의 타워 목록 정리 연결(합성에 참여한 타워도 이를 통해 제거)
    private bool TryDestroyTowerOnMaster(Tower tower)
    {
        var game = GameManager.Instance;

        if (!PhotonNetwork.IsMasterClient || tower == null || game == null || game.Container_Tower == null || !tower.photonView.IsMine)
            return false;

        int viewId = tower.photonView.ViewID;

        if (viewId <= 0)
            return false;

        try
        {
            PhotonNetwork.Destroy(tower.gameObject);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        // 호출 성공 여부 대신 현재 참여자 쪽에서 제거됐는지 확인한다.
        if (PhotonView.Find(viewId) != null)
            return false;

        game.Container_Tower.DeleteObj(tower);

        // 삭제가 끝난 뒤 목록 정리 요청에 실패해도 회수 항목을 제거하지 않는다.
        try
        {
            photonView.RPC(nameof(RpcRemoveTowerContainerEntry), RpcTarget.Others, viewId);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            PlayerInventoryRegistry.Instance?.SuspendAuthority("다른 참여자의 타워 목록 정리 요청에 실패했습니다.");
        }

        return true;
    }

    [PunRPC]
    private void RpcRemoveTowerContainerEntry(int viewId, PhotonMessageInfo info)
    {
        if (info.Sender != PhotonNetwork.MasterClient)
            return;

        var container = GameManager.Instance?.Container_Tower;

        if (container == null)
            return;

        foreach (ITower item in container.GetList())
        {
            if (item is Tower tower && tower.IndexId == viewId)
                container.DeleteObj(item);
        }
    }

    private void CompleteItemRequest(ItemRequestRecord record, bool success, string itemId)
    {
        record.Completed = true;
        record.Success = success;
        record.ItemId = success ? itemId : "";

        SendItemResult(record);
    }

    private void SendItemResult(ItemRequestRecord record)
    {
        photonView.RPC(nameof(RpcItemOperationResult), record.Requester, record.IsRecall, record.RequestId, record.Success, record.ItemId);
    }

    [PunRPC]
    private void RpcItemOperationResult(bool isRecall, string requestId, bool success, string itemId, PhotonMessageInfo info)
    {
        if (info.Sender != PhotonNetwork.MasterClient)
            return;

        DeliverItemResult(isRecall, requestId, success, itemId);
    }

    private void DeliverItemResult(bool isRecall, string requestId, bool success, string itemId)
    {
        if (isRecall)
        {
            TowerRecallResultReceived?.Invoke(requestId, success, itemId);
        }
        else
        {
            TowerSellResultReceived?.Invoke(requestId, success);
        }
    }

    private void ResetAllRequestState()
    {
        purchaseRecords.Clear();
        pendingPurchases.Clear();

        itemRequests.Clear();
        pendingSales.Clear();

        ClearBuildState();
    }
}