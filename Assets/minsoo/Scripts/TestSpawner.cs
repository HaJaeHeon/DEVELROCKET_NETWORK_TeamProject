using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] MapManager map;
    [SerializeField] TestMonster prefab;
    IReadOnlyList<Vector2> path;
    IReadOnlyList<MapPath> pathList;

    WaitForSeconds wait = new WaitForSeconds(0.5f);
    void Start()
    {
        pathList = map.GetPaths();
        path = map.GetPath(1);   // 맵 관리자가 Awake에서 계산을 마친 뒤
        Spawn();
        
    }

    private void Update()
    {
        //Spawn();
        //StartCoroutine(SpawnDelay());
        path = pathList[0].Points;
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

        PhotonNetwork.Instantiate(prefab.name, pathList[0].Points[0], Quaternion.identity);
        var m = Instantiate(prefab, pathList[0].Points[0], Quaternion.identity);
        var m2 = Instantiate(prefab, pathList[1].Points[0], Quaternion.identity);
        //m.Init(path);
    }
}