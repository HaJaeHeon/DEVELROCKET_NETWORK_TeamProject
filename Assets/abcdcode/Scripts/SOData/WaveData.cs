using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveData : SOData
{
    [Serializable]
    public class SpawnInfo
    {
        [Tooltip("웨이브 시작 후 소환 시간(초)")]
        public float time;
        [Tooltip("소환할 적 Id (EnemyDB 키)")]
        public string enemyId;
    }
    [SerializeField]private List<SpawnInfo> spawns = new List<SpawnInfo>();
    public IReadOnlyList<SpawnInfo> Spawns => spawns;
}
