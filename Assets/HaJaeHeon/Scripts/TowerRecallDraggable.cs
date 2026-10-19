using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerRecallDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Tower tower;
    // 끌기가 가능한 조건인지(소유권 확인)
    private bool canRecall;

    public Tower Tower => tower;
    public bool CanRecall => canRecall;

    private void Awake()
    {
        tower = GetComponent<Tower>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        canRecall = tower != null && tower.OwnerPlayer != null && PhotonNetwork.LocalPlayer != null &&
            tower.OwnerPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;

        if(!canRecall)
        {
            Debug.Log("다른 플레이어의 타워는 회수할 수 없습니다.");
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canRecall)
            return;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canRecall = false;
    }

    private void OnDisable()
    {
        canRecall = false;
    }
}
