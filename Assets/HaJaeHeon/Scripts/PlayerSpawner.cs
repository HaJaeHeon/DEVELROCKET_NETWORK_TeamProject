using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Image playerMousePrefab;
    [SerializeField] private Transform cursorParent;

    private void Awake()
    {
        if (playerMousePrefab == null)
            return;

        if (PhotonNetwork.PrefabPool is DefaultPool pool)
        {
            pool.ResourceCache["PlayerMouse"] = playerMousePrefab.gameObject;
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        GameObject mouse = PhotonNetwork.Instantiate("PlayerMouse", Vector3.zero, Quaternion.identity);
    }
}
