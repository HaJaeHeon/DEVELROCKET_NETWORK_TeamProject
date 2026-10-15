using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// StageData의 웨이브 목록을 순서대로 진행하며 적을 소환
/// 소환 및 웨이브 진행 판정은 마스터 클라이언트에서만 수행
/// 웨이브 소환 완료 → 필드의 적이 모두 제거됨 → WaveInterval 카운트다운 → 다음 웨이브
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
    private WaveState state;

    private enum WaveState
    {
        Spawning,   // 현재 웨이브 적 소환 중
        WaitClear,  // 소환 완료, 필드의 적이 모두 제거되길 대기
        Countdown,  // 다음 웨이브까지 카운트다운
    }
    /// <summary>
    /// 다음 웨이브까지 남은 시간(초). 카운트다운 중이 아니면 -1
    /// </summary>
    public float RemainCountdown => state == WaveState.Countdown ? Mathf.Max(0, Data.WaveInterval - timer) : -1;

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

        if(state == WaveState.Spawning)
        {
            timer += deltaTime;
            while(spawnIndex < curSpawns.Count && curSpawns[spawnIndex].time <= timer)
            {
                Spawn(curSpawns[spawnIndex].enemyId);
                spawnIndex++;
            }
            if(spawnIndex < curSpawns.Count) return;
            // 현재 웨이브 소환 완료, 적이 모두 제거될 때까지 대기
            state = WaveState.WaitClear;
        }
        if(state == WaveState.WaitClear)
        {
            if(GameManager.Instance.Container_Enemy.GetList().Count > 0) return;
            // 적 전멸, 다음 웨이브 카운트다운 시작
            state = WaveState.Countdown;
            timer = 0;
            return;
        }
        if(state == WaveState.Countdown)
        {
            timer += deltaTime;
            if(timer < Data.WaveInterval) return;
            StartWave(CurWaveIndex + 1);
        }
    }
    private void StartWave(int index)
    {
        CurWaveIndex = index;
        state = WaveState.Spawning;
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
