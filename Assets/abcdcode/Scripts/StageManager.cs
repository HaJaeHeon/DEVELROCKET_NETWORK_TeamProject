using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// StageData의 웨이브 목록을 순서대로 진행하며 적을 소환
/// 소환 및 웨이브 진행 판정은 마스터 클라이언트에서만 수행
/// 웨이브 소환 완료 → 필드의 적이 모두 제거됨 → WaveInterval 카운트다운 → 다음 웨이브
/// 진행 상태는 Room CustomProperties로 다른 클라이언트에 공유
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
    private int lastSyncedRemain = -1;

    public enum WaveState
    {
        Spawning,   // 현재 웨이브 적 소환 중
        WaitClear,  // 소환 완료, 필드의 적이 모두 제거되길 대기
        Countdown,  // 다음 웨이브까지 카운트다운
    }

    // Room CustomProperties 키
    private const string PropWaveIndex = "WaveIdx";
    private const string PropWaveState = "WaveState";
    private const string PropCountdownEnd = "WaveCdEnd";   // 카운트다운 종료 시각 (PhotonNetwork.ServerTimestamp 기준 ms)
    private const string PropRemainEnemy = "WaveRemain";

    /// <summary>
    /// 웨이브 진행을 직접 계산하는 클라이언트인지 (마스터 또는 방 밖)
    /// </summary>
    private static bool IsAuthority => PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom;

    /// <summary>
    /// 현재 웨이브 상태 (모든 클라이언트에서 조회 가능)
    /// </summary>
    public WaveState State => IsAuthority ? state : (WaveState)GetRoomProp(PropWaveState, (int)state);
    /// <summary>
    /// 현재 웨이브 인덱스 (모든 클라이언트에서 조회 가능)
    /// </summary>
    public int SyncedWaveIndex => IsAuthority ? CurWaveIndex : GetRoomProp(PropWaveIndex, CurWaveIndex);
    /// <summary>
    /// 표시용 웨이브 번호(1부터). 카운트다운 중에는 다음 웨이브 번호
    /// </summary>
    public int DisplayWaveNumber
    {
        get
        {
            int num = SyncedWaveIndex + (State == WaveState.Countdown ? 2 : 1);
            return Data == null ? num : Mathf.Min(num, Data.Waves.Count);
        }
    }
    /// <summary>
    /// 모든 웨이브 클리어 여부 (모든 클라이언트에서 조회 가능)
    /// </summary>
    public bool IsSyncedCleared => Data == null || SyncedWaveIndex >= Data.Waves.Count;
    /// <summary>
    /// 다음 웨이브까지 남은 시간(초). 카운트다운 중이 아니면 -1
    /// </summary>
    public float RemainCountdown
    {
        get
        {
            if(State != WaveState.Countdown) return -1;
            if(IsAuthority) return Mathf.Max(0, Data.WaveInterval - timer);
            int end = GetRoomProp(PropCountdownEnd, PhotonNetwork.ServerTimestamp);
            return Mathf.Max(0, unchecked(end - PhotonNetwork.ServerTimestamp) / 1000f);
        }
    }
    /// <summary>
    /// 현재 웨이브의 남은 적 수 (아직 소환되지 않은 적 + 필드의 적)
    /// </summary>
    public int RemainEnemy => IsAuthority ? CalcRemainEnemy() : GetRoomProp(PropRemainEnemy, 0);

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
            SyncRemainEnemy();
            if(spawnIndex < curSpawns.Count) return;
            // 현재 웨이브 소환 완료, 적이 모두 제거될 때까지 대기
            state = WaveState.WaitClear;
            SyncState();
        }
        if(state == WaveState.WaitClear)
        {
            SyncRemainEnemy();
            if(GameManager.Instance.Container_Enemy.GetList().Count > 0) return;
            // 적 전멸, 다음 웨이브 카운트다운 시작
            state = WaveState.Countdown;
            timer = 0;
            SyncState();
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
        if(IsCleared)
        {
            curSpawns = new List<WaveData.SpawnInfo>();
            SyncState();
            return;
        }
        WaveData wave = Data.Waves[index];
        curSpawns = wave == null ? new List<WaveData.SpawnInfo>()
            : wave.Spawns.OrderBy(s => s.time).ToList();
        SyncState();
    }
    private void Spawn(string enemyId)
    {
        int index = 0;
        Vector2 spawnpos = GameManager.Instance.IsMapDebug ? SpawnPos : GetStartTile(out index);
        var entry = GameManager.Instance.EnemyDB.Get(enemyId);
        if(entry == null || entry.prefab == null) return;
        // 적 Id를 함께 보내 모든 클라이언트에서 같은 데이터로 Init (Enemy.OnPhotonInstantiate)
        PhotonNetwork.Instantiate(entry.prefab.name, spawnpos, Quaternion.identity, 0, new object[] { enemyId,index });
    }
    private Vector2 GetStartTile(out int index)
    {
        var list = MapManager.Instance.GetPaths();
        index = Random.Range(0,list.Count);
        var result = list[index];
        
        return result.Points[0];
    }
    private int CalcRemainEnemy()
    {
        int unspawned = curSpawns == null ? 0 : curSpawns.Count - spawnIndex;
        var container = GameManager.Instance != null ? GameManager.Instance.Container_Enemy : null;
        int alive = container == null ? 0 : container.GetList().Count;
        return unspawned + alive;
    }
    /// <summary>
    /// 웨이브 인덱스/상태/카운트다운 종료 시각을 Room에 기록 (마스터 전용)
    /// </summary>
    private void SyncState()
    {
        if(!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null) return;
        int remain = CalcRemainEnemy();
        lastSyncedRemain = remain;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { PropWaveIndex, CurWaveIndex },
            { PropWaveState, (int)state },
            { PropCountdownEnd, unchecked(PhotonNetwork.ServerTimestamp + (int)(Data.WaveInterval * 1000)) },
            { PropRemainEnemy, remain },
        });
    }
    /// <summary>
    /// 남은 적 수가 바뀌었을 때만 Room에 기록 (마스터 전용)
    /// </summary>
    private void SyncRemainEnemy()
    {
        if(!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null) return;
        int remain = CalcRemainEnemy();
        if(remain == lastSyncedRemain) return;
        lastSyncedRemain = remain;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { PropRemainEnemy, remain } });
    }
    private static int GetRoomProp(string key, int fallback)
    {
        var room = PhotonNetwork.CurrentRoom;
        if(room != null && room.CustomProperties.TryGetValue(key, out object value) && value is int v)
        {
            return v;
        }
        return fallback;
    }
}
