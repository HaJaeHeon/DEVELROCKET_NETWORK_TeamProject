using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] MapManager map;
    [SerializeField] Monster prefab;
    IReadOnlyList<Vector2> path;

    WaitForSeconds wait = new WaitForSeconds(0.5f);
    void Start()
    {
        path = map.GetPath();   // 맵 관리자가 Awake에서 계산을 마친 뒤
        Spawn();
    }

    private void Update()
    {
        //Spawn();
        //StartCoroutine(SpawnDelay());

    }

    private IEnumerator SpawnDelay()
    {
        yield return wait;
    }

    public void Spawn()
    {
        if (path == null || path.Count == 0)
        {
            Debug.Log("경로가 없음");
            return;
        }
            
        var m = Instantiate(prefab, path[0], Quaternion.identity);
        m.Init(path);
    }
}