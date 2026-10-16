using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveData : SOData
{
    /// <summary>
    /// 소환 그룹. time초부터 interval초 간격으로 enemyId를 count번 소환
    /// </summary>
    [Serializable]
    public class SpawnInfo
    {
        [Tooltip("웨이브 시작 후 첫 소환 시간(초)")]
        public float time;
        [Tooltip("소환할 적 Id (EnemyDB 키)")]
        public string enemyId;
        [Tooltip("소환 횟수 (1 이하면 1번)")]
        public int count = 1;
        [Tooltip("반복 소환 간격(초)")]
        public float interval;
    }
    /// <summary>
    /// 그룹을 펼친 단일 소환 정보
    /// </summary>
    public struct SpawnEvent
    {
        public float time;
        public string enemyId;
    }
    [SerializeField]private List<SpawnInfo> spawns = new List<SpawnInfo>();
    public IReadOnlyList<SpawnInfo> Spawns => spawns;

    /// <summary>
    /// 모든 소환 그룹을 개별 소환으로 펼쳐 시간순으로 정렬한 목록
    /// </summary>
    public List<SpawnEvent> BuildTimeline()
    {
        var result = new List<SpawnEvent>();
        foreach(var info in spawns)
        {
            if(info == null) continue;
            int count = Mathf.Max(1, info.count);
            for(int i = 0; i < count; i++)
            {
                result.Add(new SpawnEvent { time = info.time + info.interval * i, enemyId = info.enemyId });
            }
        }
        return result.OrderBy(e => e.time).ToList();
    }
}
