using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class TowerService : MonoBehaviourPunCallbacks
{
    public static TowerService Instance { get; private set; }

    public bool IsBusy => isRecallPending || isSellPending;

    private SocketManager socketManager;
    // 회수 결과를 기다리는 임시 보관함
    private GameObject pendingInventoryTower;

    private bool isRecallPending;
    private string pendingRecallRequestId;

    private GameObject pendingSaleInventoryTower;
    private string pendingSaleRequestId;
    private bool isSellPending;
    private bool saleResultReceived;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        socketManager = SocketManager.Instance;

        if (socketManager == null)
        {
            Debug.LogError("타워 회수 요청에 사용할 socketManager 찾을 수 없습니다.");
            return;
        }

        socketManager.TowerRecallResultReceived += HandleRecallResult;
        socketManager.TowerSellResultReceived += HandleSellResult;
        Debug.Log("TowerService가 타워 회수 요청을 받을 준비가 되었습니다.");

        // ButtonPool 확인
        if (ButtonTowerPool.Instance == null)
        {
            Debug.LogError("ButtonTowerPool이 없습니다.");
            return;
        }
    }

    // 회수 요청
    public void RequestRecall(Tower tower)
    {
        if (!PhotonNetwork.InRoom || tower == null || socketManager == null 
            || ButtonTowerPool.Instance == null || socketManager.IsLocalOperationPending 
            || tower.photonView.ViewID <= 0)
            return;

        // 소유권 검사
        if (!IsOwnedBy(tower, PhotonNetwork.LocalPlayer))
        {
            Debug.Log("다른 플레이어의 타워는 회수할 수 없습니다.");
            return;
        }

        // 인벤토리 가득 찼는지 확인
        if (PlayerInventory.Instance == null ||
            PlayerInventory.Instance.IsFullInventory)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return;
        }

        

        // 데이터 형식 변환
        TowerData towerData = tower.Data as TowerData;

        if (towerData == null)
        {
            Debug.LogError("회수할 타워의 데이터가 없습니다.");
            return;
        }

        pendingInventoryTower = ButtonTowerPool.Instance.Spawn(ButtonTowerPool.UIType.Inventory, towerData);

        if (pendingInventoryTower == null)
            return;

        if (!PlayerInventory.Instance.TryAddTower(pendingInventoryTower))
        {
            ButtonTowerPool.Instance.Return(pendingInventoryTower);
            pendingInventoryTower = null;
            return;
        }

        // 조작 제한, 요청 전송 
        // 드래그 제한
        SetPendingTowerInteractable(false);
        // 회수 결과를 기다리는 상태로 변경
        isRecallPending = true;

        // 설치된 타워의 id 전달
        pendingRecallRequestId = System.Guid.NewGuid().ToString("N");
        socketManager.RequestTowerRecall(pendingRecallRequestId, tower.photonView.ViewID);
    }

    // 타워 판매
    public bool RecallSell(InventoryDraggable draggable)
    {
        if (!PhotonNetwork.InRoom || draggable == null || socketManager == null || socketManager.IsLocalOperationPending)
            return false;

        GameObject inventoryTower = draggable.InventoryItemObject;
        var inventory = PlayerInventory.Instance;

        if (inventoryTower == null || inventory == null || ButtonTowerPool.Instance == null || !inventory.HasTower(inventoryTower))
            return false;

        var container = draggable.GetComponent<PresetSODataContainer>();

        if (container == null || container.data == null || string.IsNullOrEmpty(container.TowerItemId))
            return false;

        pendingSaleInventoryTower = inventoryTower;
        pendingSaleRequestId = System.Guid.NewGuid().ToString("N");

        isSellPending = true;
        saleResultReceived = false;

        socketManager.RequestTowerSell(pendingSaleRequestId, container.TowerItemId);

        // 판매 완료가 아니라 요청을 시작했다는 뜻이다.
        return true;
    }
    private void HandleSellResult(string requestId, bool success)
    {
        if (!isSellPending || saleResultReceived || requestId != pendingSaleRequestId)
            return;

        saleResultReceived = true;
        StartCoroutine(FinishSellUI(requestId, success));
    }

    private IEnumerator FinishSellUI(string requestId, bool success)
    {
        // 현재 끌기 종료 처리가 원래 부모를 복원하도록 기다린다.
        yield return null;

        if (!isSellPending || requestId != pendingSaleRequestId)
            yield break;

        if (success)
        {
            var inventory = PlayerInventory.Instance;
            var pool = ButtonTowerPool.Instance;

            if (pendingSaleInventoryTower != null && inventory != null && pool != null && inventory.TryRemoveTower(pendingSaleInventoryTower))
            {
                pool.Return(pendingSaleInventoryTower);
            }
            else
            {
                Debug.LogError("판매는 승인됐지만 보관함 화면 제거에 실패했습니다.");
            }
        }
        else
        {
            Debug.Log("판매가 거절되었습니다.");
        }

        ClearSaleState();
    }

    private void ClearSaleState()
    {
        pendingSaleInventoryTower = null;
        pendingSaleRequestId = null;
        isSellPending = false;
        saleResultReceived = false;
    }


    // 연결 종료시 회수처리 cancel
    private void CancelLocalTransactions()
    {
        RollbackPendingInventoryTower();
        ClearPendingState();
        ClearSaleState();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        CancelLocalTransactions();
        base.OnDisconnected(cause);
    }

    public override void OnLeftRoom()
    {
        CancelLocalTransactions();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        CancelLocalTransactions();
    }

    private void HandleRecallResult(string requestId, bool success, string towerItemId)
    {
        if (!isRecallPending || requestId != pendingRecallRequestId || pendingInventoryTower == null)
            return;

        if (success)
        {
            PresetSODataContainer container = pendingInventoryTower.GetComponentInChildren<PresetSODataContainer>(true);
            container.SetItemId(towerItemId);

            SetPendingTowerInteractable(true);
            Debug.Log($"타워 회수 완료: {towerItemId}");
        }
        else
        {
            RollbackPendingInventoryTower();
            Debug.Log("타워 회수 실패");
        }

        ClearPendingState();
    }

    // 소유권 비교용
    private static bool IsOwnedBy(Tower tower, Player player)
    {
        return tower != null &&
            tower.OwnerPlayer != null &&
            player != null &&
            tower.OwnerPlayer.ActorNumber == player.ActorNumber;
    }

    // 임시 항목 드래그 조작 제어
    private void SetPendingTowerInteractable(bool interactable)
    {
        if (pendingInventoryTower == null)
            return;

        InventoryDraggable draggable =
            pendingInventoryTower.GetComponentInChildren<InventoryDraggable>(true);

        if (draggable != null)
            draggable.enabled = interactable;
    }

    // 임시 항목 원상 복구
    private void RollbackPendingInventoryTower()
    {
        if (pendingInventoryTower == null)
            return;

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.TryRemoveTower(pendingInventoryTower);

        if (ButtonTowerPool.Instance != null)
            ButtonTowerPool.Instance.Return(pendingInventoryTower);
    }

    // 상태 초기화
    private void ClearPendingState()
    {
        pendingInventoryTower = null;
        pendingRecallRequestId = null;
        isRecallPending = false;
    }

    private void OnDestroy()
    {
        if (socketManager != null)
        {
            socketManager.TowerRecallResultReceived -= HandleRecallResult;
            socketManager.TowerSellResultReceived -= HandleSellResult;
        }

        if (Instance == this)
            Instance = null;
    }
}
