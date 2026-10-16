using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 게임내에서 계속 유지되는 정적 데이터 DB
/// </summary>
public class StaticDataDB : SingletonBehavior<StaticDataDB>
{
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
        stageDataDic = new Dictionary<string, StageData>();
        foreach(var sd in m_StageData)
        {
            stageDataDic[sd.Id] = sd;
        }
    }
    public StageData GetData(string id)
    {
        if(stageDataDic.ContainsKey(id))
        {
            return stageDataDic[id];
        }
        return null;
    }
    private Dictionary<string, StageData> stageDataDic;
    [SerializeField]private List<StageData> m_StageData;
}