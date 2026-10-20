using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;
public class BuyTower : MonoBehaviourPunCallbacks
{
    public event Action<string, string> PurchaseRequested;

    [SerializeField] private float currentGold;

    private bool isPurchasePending;
    public bool IsPurchasePending => isPurchasePending;

    private string pendingRequestId;
    private Button pendingShopButton;
    private GameObject pendingInventoryTower;


    private void SpendToBuyTower(Button button, TowerData data)
    {
        currentGold = PlayerGold.Get(PhotonNetwork.LocalPlayer);
        Debug.Log($"현재 가진 골드 량 : {currentGold}");

        if (!PhotonNetwork.InRoom || SocketManager.Instance == null 
            || SocketManager.Instance.IsLocalOperationPending)
        {
            Debug.Log($"방이 아니거나 요청이 완료되지 않음 (요청상태 : {isPurchasePending})");
            return;
        }

        PlayerInventory inventory = PlayerInventory.Instance;
        ButtonTowerPool pool = ButtonTowerPool.Instance;

        if (inventory == null || pool == null)
            return;

        if (inventory.IsFullInventory)
        {
            Debug.LogError("인벤토리가 가득 찼습니다.");
            return;
        }

        if (currentGold < data.Cost)
        {
            Debug.Log($"금액이 모자랍니다 currentGold{currentGold} / Cost {data.Cost}");

            return;
        }
        Debug.Log("결제 진행중");

        // 아직 통신 담당이 연결되지 않았다면 구매를 시작하지 않는다.
        if (PurchaseRequested == null)
        {
            Debug.LogWarning("구매 요청을 전달할 통신 처리가 연결되지 않았습니다.");
            return;
        }

        GameObject inventoryTower =
            pool.Spawn(ButtonTowerPool.UIType.Inventory, data);

        if (inventoryTower == null)
            return;

        if (!inventory.TryAddTower(inventoryTower))
        {
            pool.Return(inventoryTower);
            return;
        }

        // 구매가 승인되기 전에는 임시 항목을 끌지 못하게 한다.
        InventoryDraggable draggable =
            inventoryTower.GetComponentInChildren<InventoryDraggable>(true);

        if (draggable != null)
            draggable.enabled = false;

        pendingInventoryTower = inventoryTower;
        pendingShopButton = button;
        pendingRequestId = System.Guid.NewGuid().ToString("N");
        isPurchasePending = true;

        button.interactable = false;

        // 요청 번호와 종류만 전달. 여기서는 결제하지 않는다.
        PurchaseRequested.Invoke(pendingRequestId, data.Id);
    }

    // 타워 데이터 가져오기
    public void GetTowerData(GameObject buttonObject)
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (buttonObject == null || buttonObject.transform.childCount == 0)
            return;

        if (!buttonObject.TryGetComponent<Button>(out var tryButton))
            return;

        Debug.Log($"{buttonObject.name} : 을 구매하기위해 클릭함");


        // 버튼 오브젝트에서 so데이터를 가져올 수 없거나 so데이터가 등록이 되어있지 않은 경우
        if (!buttonObject.transform.GetChild(0).TryGetComponent<PresetSODataContainer>(out var so) || so.data == null)
        {
            Debug.Log("so 데이터 없거나 참조가 없음");
            return;
        }
        
        // 데이터로 넣은 타워의 가격( cost) 가 0 미만일 때 
        if (so.data.Cost < 0f)
        {
            Debug.Log("가격이 0 미만입니다.");
            return;
        }

        SpendToBuyTower(tryButton, so.data);
    }

    // 통신 담당이 대표 참여자의 응답을 검증한 뒤 호출한다.
    public void HandlePurchaseResult(
        string requestId,
        bool success,
        string itemId)
    {
        // 현재 기다리는 요청의 결과만 처리한다.
        if (!isPurchasePending || requestId != pendingRequestId)
            return;

        if (!success)
        {
            CancelPendingPurchase();
            Debug.Log("구매가 거절되었습니다.");
            return;
        }

        if (string.IsNullOrEmpty(itemId) || pendingInventoryTower == null)
        {
            Debug.LogError("구매 승인 정보가 올바르지 않습니다.");
            return;
        }

        PresetSODataContainer container =
            pendingInventoryTower
                .GetComponentInChildren<PresetSODataContainer>(true);

        if (container == null)
        {
            Debug.LogError("구매 항목의 정보 구성 요소(component)가 없습니다.");
            return;
        }

        // Registry에서 발급한 항목 고유 식별자
        container.SetItemId(itemId);

        InventoryDraggable draggable =
            pendingInventoryTower
                .GetComponentInChildren<InventoryDraggable>(true);

        if (draggable != null)
            draggable.enabled = true;

        // 승인된 보관함 항목은 남기고 상점 상품만 반환한다.
        if (pendingShopButton != null)
            ButtonTowerPool.Instance.Return(pendingShopButton.gameObject);

        ClearPurchaseState();

        Debug.Log("구매 승인과 보관함 등록이 완료되었습니다.");
    }

    private void CancelPendingPurchase()
    {
        if (pendingInventoryTower != null)
        {
            if (PlayerInventory.Instance != null)
                PlayerInventory.Instance.TryRemoveTower(pendingInventoryTower);

            if (ButtonTowerPool.Instance != null)
                ButtonTowerPool.Instance.Return(pendingInventoryTower);
        }

        if (pendingShopButton != null)
            pendingShopButton.interactable = true;

        ClearPurchaseState();
    }

    private void ClearPurchaseState()
    {
        pendingInventoryTower = null;
        pendingShopButton = null;
        pendingRequestId = null;
        isPurchasePending = false;
    }

    public override void OnLeftRoom()
    {
        CancelPendingPurchase();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        CancelPendingPurchase();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        CancelPendingPurchase();
        base.OnDisconnected(cause);
    }
}
