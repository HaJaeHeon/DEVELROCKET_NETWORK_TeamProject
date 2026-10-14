using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private void Awake()
    {
        if (!PhotonNetwork.InRoom)
            return;

        PhotonNetwork.Instantiate("PlayerMousePointer", Vector3.zero, Quaternion.identity);
    }
}
