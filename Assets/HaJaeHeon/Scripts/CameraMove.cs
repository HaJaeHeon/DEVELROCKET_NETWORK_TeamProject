using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine.EventSystems;


/// <summary>
/// 카메라 이동 범위 제한 존재해야함
/// </summary>


public class CameraMove : MonoBehaviourPunCallbacks
{
    private Camera myCamera;

    public float cameraSpeed;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        myCamera = Camera.main;

        // 2d raycast 추가
        if(myCamera.TryGetComponent<Physics2DRaycaster>(out var rayCast))
        {
            rayCast.enabled = true;
        }
        else
        {
            myCamera.AddComponent<Physics2DRaycaster>();
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (myCamera == null)
            return;

        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);

        myCamera.transform.position += moveDirection.normalized * cameraSpeed * Time.deltaTime;
    }
}
