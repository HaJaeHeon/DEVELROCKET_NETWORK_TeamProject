using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class NetworkPrefabPool : MonoBehaviour, IPunPrefabPool
{
    [SerializeField]private List<GameObject> m_towerPrefab = new List<GameObject>();
    [SerializeField]private List<GameObject> m_enemyPrefab = new List<GameObject>();
    private Dictionary<string, GameObject> prefabMap;
    private readonly Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();

    public void Register()
    {
        prefabMap = new Dictionary<string, GameObject>();
        Register(m_towerPrefab);
        Register(m_enemyPrefab);
        PhotonNetwork.PrefabPool = this;
    }
    /// <summary>
    /// 외부(EnemyDB 등)에서 프리팹을 추가 등록
    /// </summary>
    public void Register(IEnumerable<GameObject> list)
    {
        foreach (var p in list)
        {
            if (p == null) continue;
            if (prefabMap.TryGetValue(p.name, out var exist) && exist != p)
                Debug.LogWarning($"[NetworkPrefabPool] 이름이 같은 다른 프리팹이 덮어씌워짐: {p.name}");
            prefabMap[p.name] = p;
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (pool.TryGetValue(prefabId, out var q) && q.Count > 0)
        {
            var reused = q.Dequeue();
            reused.transform.SetPositionAndRotation(position, rotation);
            return reused; // 이미 비활성 상태
        }

        if (!prefabMap.TryGetValue(prefabId, out var prefab))
        {
            Debug.LogError($"[NetworkPrefabPool] 등록되지 않은 프리팹: {prefabId}");
            return null;
        }

        var go = Object.Instantiate(prefab, position, rotation);
        go.name = prefabId; // "(Clone)" 제거, Destroy 시 키로 사용
        go.SetActive(false); // PUN이 ViewID 할당 후 활성화하므로 비활성으로 반환
        return go;
    }

    public void Destroy(GameObject go)
    {
        go.SetActive(false);
        if (!pool.TryGetValue(go.name, out var q))
            pool[go.name] = q = new Queue<GameObject>();
        q.Enqueue(go);
    }
}
