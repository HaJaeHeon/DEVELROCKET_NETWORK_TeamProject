using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TowerService : MonoBehaviourPunCallbacks
{
    public static TowerService Instance { get; private set; }

    private TilemapSocket networkGateway;
    private GameObject pendingInventoryTower;
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
        networkGateway = FindAnyObjectByType<TilemapSocket>();

        if (networkGateway == null)
        {
            Debug.LogError("타워 회수 요청에 사용할 TilemapSocket을 찾을 수 없습니다.");
            return;
        }

        networkGateway.TowerRecallResultReceived += HandleRecallResult;
        Debug.Log("TowerService가 타워 회수 요청을 받을 준비가 되었습니다.");
    }

    public void RequestRecall(Tower tower)
    {
        if (!PhotonNetwork.InRoom ||
            tower == null ||
            isRecallPending ||
            networkGateway == null)
            return;

        if (!IsOwnedBy(tower, PhotonNetwork.LocalPlayer))
        {
            Debug.Log("다른 플레이어의 타워는 회수할 수 없습니다.");
            return;
        }

        if (PlayerInventory.Instance == null ||
            PlayerInventory.Instance.IsFullInventory)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return;
        }

        if (ButtonTowerPool.Instance == null)
        {
            Debug.LogError("ButtonTowerPool을 찾을 수 없습니다.");
            return;
        }

        TowerData towerData = tower.Data as TowerData;

        if (towerData == null)
        {
            Debug.LogError("회수할 타워의 데이터를 찾을 수 없습니다.");
            return;
        }

        pendingInventoryTower = ButtonTowerPool.Instance.Spawn(
            ButtonTowerPool.UIType.Inventory,
            towerData);

        if (!PlayerInventory.Instance.TryAddTower(pendingInventoryTower))
        {
            ButtonTowerPool.Instance.Return(pendingInventoryTower);
            pendingInventoryTower = null;
            return;
        }

        SetPendingTowerInteractable(false);
        isRecallPending = true;

        networkGateway.RequestTowerRecall(tower.photonView.ViewID);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        RollbackPendingInventoryTower();
        ClearPendingState();
        base.OnDisconnected(cause);
    }

    private void OnDestroy()
    {
        if (networkGateway != null)
            networkGateway.TowerRecallResultReceived -= HandleRecallResult;

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

    private static bool IsOwnedBy(Tower tower, Player player)
    {
        return tower != null &&
            tower.OwnerPlayer != null &&
            player != null &&
            tower.OwnerPlayer.ActorNumber == player.ActorNumber;
    }

    private void SetPendingTowerInteractable(bool interactable)
    {
        if (pendingInventoryTower == null)
            return;

        InventoryDraggable draggable =
            pendingInventoryTower.GetComponentInChildren<InventoryDraggable>(true);

        if (draggable != null)
            draggable.enabled = interactable;
    }

    private void RollbackPendingInventoryTower()
    {
        if (pendingInventoryTower == null)
            return;

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.TryRemoveTower(pendingInventoryTower);

        if (ButtonTowerPool.Instance != null)
            ButtonTowerPool.Instance.Return(pendingInventoryTower);
    }

    private void ClearPendingState()
    {
        pendingInventoryTower = null;
        isRecallPending = false;
    }
}
