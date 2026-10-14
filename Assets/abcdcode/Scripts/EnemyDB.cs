using System;
using System.Collections.Generic;
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
}
