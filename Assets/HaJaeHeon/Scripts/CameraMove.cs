using UnityEngine;
using Photon.Pun;

public class CameraMove : MonoBehaviourPunCallbacks
{
    private Camera myCamera;

    public float cameraSpeed;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        myCamera = Camera.main;
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
