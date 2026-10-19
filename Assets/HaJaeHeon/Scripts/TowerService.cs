using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TowerService : MonoBehaviourPunCallbacks
{
    public static TowerService Instance { get; private set; }

    private TilemapSocket tileMapSocket;
    // 회수 결과를 기다리는 임시 보관함
    private GameObject pendingInventoryTower;
    // 회수 응답을 기다리고있는지
    private bool isRecallPending;

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
        tileMapSocket = FindAnyObjectByType<TilemapSocket>();

        if (tileMapSocket == null)
        {
            Debug.LogError("타워 회수 요청에 사용할 TilemapSocket을 찾을 수 없습니다.");
            return;
        }

        tileMapSocket.TowerRecallResultReceived += HandleRecallResult;
        Debug.Log("TowerService가 타워 회수 요청을 받을 준비가 되었습니다.");
    }

    // 회수 요청
    public void RequestRecall(Tower tower)
    {
        if (!PhotonNetwork.InRoom || tower == null ||
            isRecallPending || tileMapSocket == null)
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

        // ButtonPool 확인
        if (ButtonTowerPool.Instance == null)
        {
            Debug.LogError("ButtonTowerPool이 없습니다.");
            return;
        }

        // 데이터 형식 변환
        TowerData towerData = tower.Data as TowerData;

        if (towerData == null)
        {
            Debug.LogError("회수할 타워의 데이터가 없습니다.");
            return;
        }

        pendingInventoryTower = ButtonTowerPool.Instance.Spawn(
            ButtonTowerPool.UIType.Inventory, towerData);

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
        tileMapSocket.RequestTowerRecall(tower.photonView.ViewID);
    }

    // 연결 종료시 회수처리 cancel
    public override void OnDisconnected(DisconnectCause cause)
    {
        RollbackPendingInventoryTower();
        ClearPendingState();
        base.OnDisconnected(cause);
    }

    private void OnDestroy()
    {
        if (tileMapSocket != null)
            tileMapSocket.TowerRecallResultReceived -= HandleRecallResult;

        if (Instance == this)
            Instance = null;
    }

    private void HandleRecallResult(bool success)
    {
        if (!isRecallPending || pendingInventoryTower == null)
            return;

        if (success)
        {
            SetPendingTowerInteractable(true);
            Debug.Log("타워 회수 완료");
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
        isRecallPending = false;
    }
}
