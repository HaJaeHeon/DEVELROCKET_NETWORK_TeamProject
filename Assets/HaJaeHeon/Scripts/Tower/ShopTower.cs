using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using static ButtonTowerPool;

public class ShopTower : MonoBehaviourPunCallbacks
{
    private readonly List<TowerData> towerList = new();

    private StageManager stageManager;
    private GameManager gameManager;
    private ButtonTowerPool pool;

    private void Start()
    {
        gameManager = GameManager.Instance;
        stageManager = GameManager.Instance.StageManager;
        pool = ButtonTowerPool.Instance;

        if (stageManager != null)
            stageManager.OnWaveEnd += SendReroll;
    }

    private void OnDestroy()
    {
        if(stageManager != null)
            stageManager.OnWaveEnd -= SendReroll;
    }


    public void Initialize()
    {
        towerList.Clear();

        foreach (TowerDB.Entry entry in GameManager.Instance.TowerDB.Map.Values)
        {
            if (entry == null || entry.data == null || entry.prefab == null)
                continue;

            if (entry.data.Level == 1 && entry.id == entry.data.Id)
                towerList.Add(entry.data);
        }
    }

    public bool HasCandidates => towerList.Count > 0;

    public TowerData GetRandomTower()
    {
        if (!HasCandidates)
        {
            Debug.Log("여기서 return");
            return null;
        }
        return towerList[Random.Range(0, towerList.Count)];
    }

    private void SendReroll(int waveNum)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            return;

        photonView.RPC(nameof(RerollShopRpc), RpcTarget.All, waveNum);
    }

    private void RerollShop(int waveNum)
    {
        int wave = Mathf.Clamp(waveNum +1 , 1, 3);

        var candidates = new List<TowerData>();

        foreach (TowerDB.Entry entry in GameManager.Instance.TowerDB.Map.Values)
        {
            if (entry == null || entry.data == null || entry.prefab == null)
                continue;

            if (entry.data.Level == 1 && entry.id == entry.data.Id)
                candidates.Add(entry.data);
            //if (entry.data.Level <= wave && entry.id == entry.data.Id)
            //    candidates.Add(entry.data);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"{wave}단계 상점 후보가 없습니다.");
            return;
        }


        towerList.Clear();
        towerList.AddRange(candidates);

        pool.ClearShopObjects();
        
        for (int i = 0; i < SHOPMAXCOUNT; i++)
        {
            TowerData data = GetRandomTower();
            pool.Spawn(UIType.Shop, data);
        }

        Debug.Log("상점 리롤");
    }

    [PunRPC]
    private void RerollShopRpc(int waveNum, PhotonMessageInfo info)
    {
        if (info.Sender == null || info.Sender != PhotonNetwork.MasterClient || waveNum < 0)
        {
            Debug.Log("여기 return");
            return;
        }

        RerollShop(waveNum);
    }
}
