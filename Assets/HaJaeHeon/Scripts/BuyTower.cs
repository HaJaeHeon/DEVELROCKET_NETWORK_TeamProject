using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class BuyTower : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private float currentGold;

    private float targetGold;

    private void SpendToBuyTower(Button button)
    {
        currentGold = PlayerGold.Get(PhotonNetwork.LocalPlayer);
        Debug.Log($"현재 가진 골드 량 : {currentGold}");

        if (currentGold < targetGold)
        {
            Debug.Log($"금액이 모자랍니다 currentGold{currentGold} / targetGold {targetGold}");

            button.interactable = true;
            return;
        }
        Debug.Log("결제 진행중");

        float remainGold = currentGold - targetGold;

        PlayerGold.Set(remainGold);

        Destroy(button.gameObject);
    }

    public void GetTowerData(GameObject buttonObject)
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (buttonObject == null || buttonObject.transform.childCount == 0)
            return;

        if (!buttonObject.TryGetComponent<Button>(out var tryButton))
            return;

        Button button = buttonObject.GetComponent<Button>();
        button.interactable = false;

        Debug.Log($"{buttonObject.name} : 을 구매하기위해 클릭함");

        if (!buttonObject.transform.GetChild(0).TryGetComponent<PresetSODataContainer>(out var so) || so.data == null)
        {
            Debug.Log("so 데이터 없거나 참조가 없음");
            button.interactable = true;
            return;
        }

        if (so.data.Cost < 0f)
        {
            Debug.Log("가격이 0 미만입니다.");
            button.interactable = true;
            return;
        }

        targetGold = so.data.Cost;
        Debug.Log($"타워의 가격 : {targetGold}");

        SpendToBuyTower(button);
    }
}
