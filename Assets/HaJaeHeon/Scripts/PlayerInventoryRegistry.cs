using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// masterClient 의 사용자별 보유 기록 / 화면 객체,재화,구매 이력을 별도로 관리
public class PlayerInventoryRegistry : MonoBehaviourPunCallbacks
{
    public static PlayerInventoryRegistry Instance { get; private set; }

    public enum ItemOperation { Build, Sell }

    private sealed class Record
    {
        public InventoryTowerItem Item;
        public string ReservationId;
        public ItemOperation Operation;
    }

    private readonly Dictionary<int, Dictionary<string, Record>> inventories = new();
    private bool authorityReady = true;

    public bool CanManage => authorityReady && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private bool IsCurrentPlayer(Player player)
    {
        return CanManage && player != null && PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.GetPlayer(player.ActorNumber) == player;
    }

    // 구매·회수를 승인한 뒤 한 번만 등록한다. 대기 중인 항목도 자리를 차지함
    public bool TryAddItem(Player player, TowerData data, out InventoryTowerItem item)
    {
        item = null;
        if (!IsCurrentPlayer(player) || data == null || string.IsNullOrEmpty(data.Id) ||
            data.Level < 1 || float.IsNaN(data.Cost) || float.IsInfinity(data.Cost) || data.Cost < 0f)
            return false;

        if (!inventories.TryGetValue(player.ActorNumber, out var inventory))
        {
            inventory = new Dictionary<string, Record>();
            inventories.Add(player.ActorNumber, inventory);
        }
        if (inventory.Count >= PlayerInventory.MAXINVENTORYCOUNT) return false;
        
        Debug.Log($"[항목 생성 전] 사용자={player.ActorNumber}, 타워={data.Id}, 단계={data.Level}, 가격={data.Cost}");
        item = new InventoryTowerItem(Guid.NewGuid().ToString("N"), data.Id, data.Cost, data.Level);
        Debug.Log($"[항목 생성 후] 사용자={player.ActorNumber}, 항목={item.ItemId}, 타워={item.TowerId}, 단계={item.TowerLevel}, 가격={item.TowerPrice}");
        
        inventory.Add(item.ItemId, new Record { Item = item });
        return true;
    }

    // 이번 처리에서 새로 등록한 항목의 복구용.
    // 사용자 요청을 그대로 받아 호출하면 안 됨.
    public bool TryRollbackAddedItem(Player player, string itemId)
    {
        if (!TryGetRecord(player, itemId, out var record))
            return false;

        if (record.ReservationId != null)
            return false;

        return inventories[player.ActorNumber].Remove(itemId);
    }

    public bool TryGetItem(Player player, string itemId, out InventoryTowerItem item)
    {
        item = null;
        if (!TryGetRecord(player, itemId, out var record)) return false;
        item = record.Item;
        return true;
    }

    // 원본 목록을 변경할 수 없도록 읽기 전용 복사본을 반환
    public IReadOnlyList<InventoryTowerItem> GetItems(Player player)
    {
        var result = new List<InventoryTowerItem>();
        if (IsCurrentPlayer(player) && inventories.TryGetValue(player.ActorNumber, out var inventory))
            foreach (var record in inventory.Values) result.Add(record.Item);
        return result.AsReadOnly();
    }

    // 설치·판매 전 예약 같은 항목의 동시 사용을 막음
    public bool TryReserveItem(Player player, string itemId, ItemOperation operation,
        out string reservationId, out InventoryTowerItem item)
    {
        reservationId = null;
        item = null;
        if ((operation != ItemOperation.Build && operation != ItemOperation.Sell) ||
            !TryGetRecord(player, itemId, out var record) || record.ReservationId != null)
            return false;

        reservationId = Guid.NewGuid().ToString("N");
        record.ReservationId = reservationId;
        record.Operation = operation;
        item = record.Item;
        return true;
    }

    // 설치·판매 성공 시 보유 기록에서 제거 / 예약 번호가 일치해야 함
    public bool TryConsumeItem(Player player, string itemId, string reservationId)
    {
        if (!TryGetReservation(player, itemId, reservationId, out _)) return false;
        return inventories[player.ActorNumber].Remove(itemId);
    }

    // 실패 시 항목은 유지하고 예약만 해제
    public bool TryCancelReservation(Player player, string itemId, string reservationId)
    {
        if (!TryGetReservation(player, itemId, reservationId, out var record)) return false;
        record.ReservationId = null;
        return true;
    }

    private bool TryGetRecord(Player player, string itemId, out Record record)
    {
        record = null;
        return IsCurrentPlayer(player) && !string.IsNullOrEmpty(itemId) &&
            inventories.TryGetValue(player.ActorNumber, out var inventory) &&
            inventory.TryGetValue(itemId, out record);
    }

    private bool TryGetReservation(Player player, string itemId, string reservationId, out Record record)
    {
        return TryGetRecord(player, itemId, out record) &&
            !string.IsNullOrEmpty(reservationId) && record.ReservationId == reservationId;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 재접속 대기 중인 사용자는 유지
        if (CanManage && otherPlayer != null && !otherPlayer.IsInactive)
            inventories.Remove(otherPlayer.ActorNumber);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // 기록 동기화 연결 전에는 빈 기록으로 거래를 처리 x
        authorityReady = false;
        if (PhotonNetwork.IsMasterClient)
            Debug.LogWarning("대표 참여자가 변경되었습니다. 보유 기록 복원 기능을 연결해야 거래를 재개할 수 있습니다.");
    }

    public override void OnLeftRoom()
    {
        inventories.Clear();
        authorityReady = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        inventories.Clear();
        authorityReady = false;
    }

    public override void OnJoinedRoom()
    {
        inventories.Clear();
        authorityReady = true;
    }
}
