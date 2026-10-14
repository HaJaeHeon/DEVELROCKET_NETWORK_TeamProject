using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// StageData의 웨이브 목록을 순서대로 진행하며 적을 소환
/// 소환은 마스터 클라이언트에서만 수행
/// </summary>
public class StageManager
{
    public StageData Data { get; private set; }
    public int CurWaveIndex { get; private set; }
    public bool IsCleared => Data == null || CurWaveIndex >= Data.Waves.Count;

    // TODO: 맵의 Start 타일 위치로 교체
    public Vector2 SpawnPos = new Vector2(-3, -3);

    private List<WaveData.SpawnInfo> curSpawns;
    private int spawnIndex;
    private float timer;
    private bool isWaiting;

    public void Init(StageData data)
    {
        Data = data;
        CurWaveIndex = 0;
        if(Data == null)
        {
            Debug.LogWarning("StageManager: StageData가 없습니다.");
            return;
        }
        StartWave(0);
    }
    public void Update(float deltaTime)
    {
        if(!PhotonNetwork.IsMasterClient || IsCleared) return;

        timer += deltaTime;
        if(isWaiting)
        {
            if(timer < Data.WaveInterval) return;
            StartWave(CurWaveIndex + 1);
            if(IsCleared) return;
        }
        while(spawnIndex < curSpawns.Count && curSpawns[spawnIndex].time <= timer)
        {
            Spawn(curSpawns[spawnIndex].enemyId);
            spawnIndex++;
        }
        if(spawnIndex >= curSpawns.Count)
        {
            // 현재 웨이브 소환 완료, 다음 웨이브 대기
            isWaiting = true;
            timer = 0;
        }
    }
    private void StartWave(int index)
    {
        CurWaveIndex = index;
        isWaiting = false;
        timer = 0;
        spawnIndex = 0;
        if(IsCleared) return;
        WaveData wave = Data.Waves[index];
        curSpawns = wave == null ? new List<WaveData.SpawnInfo>()
            : wave.Spawns.OrderBy(s => s.time).ToList();
    }
    private void Spawn(string enemyId)
    {
        var entry = GameManager.Instance.EnemyDB.Get(enemyId);
        if(entry == null || entry.prefab == null) return;
        // 적 Id를 함께 보내 모든 클라이언트에서 같은 데이터로 Init (Enemy.OnPhotonInstantiate)
        PhotonNetwork.Instantiate(entry.prefab.name, SpawnPos, Quaternion.identity, 0, new object[] { enemyId });
    }
}
