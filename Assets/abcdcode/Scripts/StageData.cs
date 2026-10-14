using System.Collections.Generic;
using UnityEngine;

public class StageData : SOData
{
    [SerializeField]private List<WaveData> waves = new List<WaveData>();
    [Tooltip("이전 웨이브의 마지막 소환 후 다음 웨이브 시작까지 대기 시간(초)")]
    [SerializeField]private float waveInterval = 5;
    public IReadOnlyList<WaveData> Waves => waves;
    public float WaveInterval => waveInterval;
}
