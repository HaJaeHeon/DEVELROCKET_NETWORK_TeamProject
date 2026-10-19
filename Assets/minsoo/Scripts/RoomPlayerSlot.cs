using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomPlayerSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject readyMark;    // "READY" 표시
    //[SerializeField] private GameObject masterMark;   // 방장 표시 (선택)

    public void Set(Player player)
    {
        bool hasPlayer = player != null;

        nameText.text = hasPlayer ? player.NickName : "Empty";
        readyMark.SetActive(hasPlayer && NetworkManager.IsReady(player));
        //if (masterMark != null)
        //    masterMark.SetActive(hasPlayer && player.IsMasterClient);
    }
}