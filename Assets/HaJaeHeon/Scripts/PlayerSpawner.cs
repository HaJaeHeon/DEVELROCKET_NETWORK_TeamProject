using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerMousePrefab;
    [SerializeField] private MyColor initialColor;

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

        //// 이미 선택한 색상이 있으면 유지합니다.
        //if (!PhotonNetwork.LocalPlayer.CustomProperties
        //        .ContainsKey("PlayerColor"))
        //{
        //    PhotonNetwork.LocalPlayer.SetCustomProperties(
        //        new Hashtable
        //        {
        //            { "PlayerColor", (int)initialColor }
        //        }
        //    );
        //}

        PhotonNetwork.Instantiate("PlayerMousePointer", Vector3.zero, Quaternion.identity);
    }
}
