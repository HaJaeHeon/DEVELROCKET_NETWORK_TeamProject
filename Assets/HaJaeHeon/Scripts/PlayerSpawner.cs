using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerMousePrefab;

    private void Awake()
    {
        if (playerMousePrefab == null)
            return;

        if (PhotonNetwork.PrefabPool is DefaultPool pool)
        {
            pool.ResourceCache["PlayerMousePointer"] = playerMousePrefab;
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        PhotonNetwork.Instantiate("PlayerMousePointer", Vector3.zero, Quaternion.identity);
    }
}
