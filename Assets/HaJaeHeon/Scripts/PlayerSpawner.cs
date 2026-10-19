using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject localPlayerMousePrefab;
    [SerializeField] private GameObject networkPlayerMousePrefab;
    [SerializeField] private Transform cursorParent;

    private void Awake()
    {
        if (networkPlayerMousePrefab == null)
            return;

        if (PhotonNetwork.PrefabPool is DefaultPool pool)
        {
            pool.ResourceCache["NetworkPlayerMouse"] = networkPlayerMousePrefab.gameObject;
        }
    }

    private void Start()
    {
        //if (!PhotonNetwork.InRoom)
        //    return;

        Cursor.visible = false;

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "StartScene")
        {
            SpawnLocalCursor();
            return;
        }

        if (currentSceneName == "GameScene" && PhotonNetwork.InRoom)
        {
            SpawnNetworkCursor();
            return;
        }
    }

    private void SpawnLocalCursor()
    {
        GameObject mouse = Instantiate(localPlayerMousePrefab, Vector3.zero, Quaternion.identity);

        Image mouseImage = mouse.GetComponentInChildren<Image>();

        if (mouseImage != null)
            mouseImage.color = Color.white;
    }

    private void SpawnNetworkCursor()
    {
        GameObject mouse = PhotonNetwork.Instantiate("NetworkPlayerMouse", Vector3.zero, Quaternion.identity);
    }
}
