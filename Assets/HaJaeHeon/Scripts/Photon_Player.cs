using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Photon_Player : MonoBehaviourPunCallbacks
{
    // 카메라 캐싱
    private Camera mainCamera;

    private void Start()
    {
        // 플레이어 본인이 사용할 카메라 캐싱
        if (photonView.IsMine)
        {
            mainCamera = Camera.main;
            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }



    private void Update()
    {
        // 플레이어 본인이 아닌 경우 , 카메라 캐싱 안되어있으면 return;
        if (!photonView.IsMine || mainCamera == null)
            return;

        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z =
            mainCamera.WorldToScreenPoint(transform.position).z;

        Vector3 target =
            mainCamera.ScreenToWorldPoint(mousePosition);

        target.z = transform.position.z;

        transform.position = target;
    }
}
