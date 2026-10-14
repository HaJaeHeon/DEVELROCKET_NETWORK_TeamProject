using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Photon_Player : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject mousePrefab;
    
    private GameObject playerMouse;


    // 카메라 캐싱|
    private Camera mainCamera;

    private void Start()
    {
        // 방에 없으면 리턴
        if (!PhotonNetwork.InRoom)
            return;

        if (!photonView.IsMine)
            return;

        mainCamera = Camera.main;

        // 프리팹 없거나 카메라 캐싱 안되면 return;
        if (mousePrefab == null || mainCamera == null)
        {
            Debug.LogError("프리팹이 없거나 카메라 캐싱 안됨");
            return;
        }

        //playerMouse = Instantiate(mousePrefab);
        playerMouse = PhotonNetwork.Instantiate("mousePrefab", Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (!photonView.IsMine)
            return;

        OwnerMouseMove();
    }

    private void OwnerMouseMove()
    {
        if (playerMouse == null || mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z =
            mainCamera.WorldToScreenPoint(playerMouse.transform.position).z;

        Vector3 target =
            mainCamera.ScreenToWorldPoint(mousePosition);

        target.z = playerMouse.transform.position.z;

        playerMouse.transform.position = target;
    }
}
