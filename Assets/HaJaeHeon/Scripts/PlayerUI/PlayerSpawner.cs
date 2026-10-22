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
        //if (networkPlayerMousePrefab == null)
        //    return;

        //if (PhotonNetwork.PrefabPool is DefaultPool pool)
        //{
        //    pool.ResourceCache["NetworkPlayerMouse"] = networkPlayerMousePrefab.gameObject;
        //}
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

        if(currentSceneName == "EditorScene")
        {
            SpawnLocalCursor();
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
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[마우스 생성 중단] 현재 방에 입장하지 않았습니다.");
            return;
        }

        if (networkPlayerMousePrefab == null)
        {
            Debug.LogError("[마우스 생성 중단] 원본 객체가 연결되지 않았습니다.");
            return;
        }

        Debug.Log($"[마우스 생성 요청] 이름={networkPlayerMousePrefab.name}, 보관소={PhotonNetwork.PrefabPool.GetType().Name}");

        GameObject mouse = PhotonNetwork.Instantiate(networkPlayerMousePrefab.name, Vector3.zero, Quaternion.identity);

        if (mouse == null)
        {
            Debug.LogError("[마우스 생성 실패] 원본 객체 등록과 Photon 오류를 확인하세요.");
            return;
        }
    }
}
