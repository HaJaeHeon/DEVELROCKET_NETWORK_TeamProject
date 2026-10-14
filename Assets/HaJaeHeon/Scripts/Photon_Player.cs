using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Photon_Player : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerMouse;


    // 게임 씬에 들어오면 플레이어 마우스 따라 움직이는 이미지 생기게끔
    private void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        playerMouse = PhotonNetwork.Instantiate("PlayerMouse", Vector2.zero, Quaternion.identity);
    }

    private void Update()
    {
        ChaseOnwerMouse();


    }


    // 플레이어 마우싀 위치에 따라서 
    private void ChaseOnwerMouse()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Destroy(gameObject);
            return;
        }
        if (!PhotonNetwork.InRoom)
        {
            Destroy(gameObject);
            return;
        }

        if (!photonView.IsMine)
            return;

        // 사람마다 해상도가 다른데 그럼 이거는 상관이 없는가?
        

    }
}
