using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class BuyTower : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private float currentGold;
    private float reflectedGold;

    private bool isRequestComplete;

    private float targetGold;

    // 인벤토리 상태 확인 후 결제하는 부분 필요
    // 지금은 그냥 버튼을 삭제하는데 이걸 objectpool 해야함

    private void SpendToBuyTower(Button button, TowerData data)
    {
        currentGold = PlayerGold.Get(PhotonNetwork.LocalPlayer);
        Debug.Log($"현재 가진 골드 량 : {currentGold}");
        
        if(!PhotonNetwork.InRoom || isRequestComplete)
        {
            Debug.Log($"방이 아니거나 요청이 완료되지 않음 (요청상태 : {isRequestComplete})");
            return;
        }

        if (currentGold < targetGold)
        {
            Debug.Log($"금액이 모자랍니다 currentGold{currentGold} / targetGold {targetGold}");

            return;
        }

        if(PlayerInventory.Instance.IsFullInventory)
        {
            Debug.LogError("인벤토리가 가득 찼습니다.");
            return;
        }

        Debug.Log("결제 진행중");

        reflectedGold = currentGold - targetGold;
        isRequestComplete = true;

        if(PlayerGold.TrySpend(targetGold))
        {
            Debug.Log($"골드 소모 요청이 완료되었습니다 원래 골드 : {currentGold} / 소모한 골드 : {targetGold} / 반영 골드 : {reflectedGold} ");
        }
        else
        {
            isRequestComplete = false;
            Debug.Log("골드가 부족하여 요청이 거절되었습니다.");
            return;
        }
        
        GameObject inventoryTower = ButtonTowerPool.Instance.Spawn(ButtonTowerPool.UIType.Inventory, data);
        // 인벤토리에 추가 못한다면
        if (!PlayerInventory.Instance.TryAddTower(inventoryTower))
        {
            ButtonTowerPool.Instance.Return(inventoryTower);
            PlayerGold.Add(targetGold);
            isRequestComplete = false;

            return;
        }

        ButtonTowerPool.Instance.Return(button.gameObject);
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

        Button button = buttonObject.GetComponent<Button>();

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

        targetGold = so.data.Cost;
        Debug.Log($"타워의 가격 : {targetGold}");

        SpendToBuyTower(button, so.data);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer != PhotonNetwork.LocalPlayer)
            return;

        if (!changedProps.TryGetValue(PlayerGold.Key, out object value) || !(value is float gold))
            return;

        Debug.Log($" 반영된 재화 : {gold}");

        if (!isRequestComplete)
            return;

        if(gold == reflectedGold)
        {
            isRequestComplete = false;
            Debug.Log($"reflectedGold : {reflectedGold} / propertiesGold : {gold} 골드가 반영되었습니다.");
        }
    }
}
