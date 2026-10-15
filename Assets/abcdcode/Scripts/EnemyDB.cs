using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 적 Id -> (외형 프리팹, 스탯 데이터) 쌍을 보관
/// </summary>
public class EnemyDB : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string id;
        public GameObject prefab;
        public EnemyData data;
    }
    [SerializeField]private List<Entry> entries = new List<Entry>();
    private Dictionary<string, Entry> map;

    public void Awake()
    {
        Build();
    }
    private void Build()
    {
        map = new Dictionary<string, Entry>();
        foreach (var e in entries)
        {
            if(e == null || string.IsNullOrEmpty(e.id)) continue;
            if(map.ContainsKey(e.id))
            {
                Debug.LogWarning($"[EnemyDB] 중복된 Id: {e.id}");
                continue;
            }
            map[e.id] = e;
        }
    }
    public IEnumerable<GameObject> Prefabs
    {
        get
        {
            foreach (var e in entries)
            {
                if(e != null && e.prefab != null) yield return e.prefab;
            }
        }
    }
    public Entry Get(string id)
    {
        // 다른 오브젝트의 Awake에서 먼저 호출될 수 있으므로 지연 생성
        if(map == null) Build();
        if(!map.TryGetValue(id, out var entry))
        {
            Debug.LogError($"[EnemyDB] 등록되지 않은 적 Id: {id}");
            return null;
        }
        return entry;
    }
    /// <summary>
    /// tile 위치에 id에 해당하는 적을 네트워크 생성 (마스터 전용)
    /// </summary>
    public Enemy Create(ITileMap tile, string id)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning($"[EnemyDB] 적 생성은 마스터 클라이언트만 가능: {id}");
            return null;
        }
        var entry = Get(id);
        if(entry == null || entry.prefab == null) return null;
        // 적 Id를 함께 보내 모든 클라이언트에서 같은 데이터로 Init (Enemy.OnPhotonInstantiate)
        var go = PhotonNetwork.Instantiate(entry.prefab.name, tile.Pos, Quaternion.identity, 0, new object[] { id });
        return go != null ? go.GetComponent<Enemy>() : null;
    }
}
