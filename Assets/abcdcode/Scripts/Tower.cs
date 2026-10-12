using System.Security.Cryptography;
using Photon.Pun;
using UnityEngine;

public class Tower : MultiMono,ITower,IPacketer
{
    public virtual void OnClickDemo()
    {
        if(photonView.IsMine)
        {
            this.Delete();
        }
        else
        {
            UnityEngine.Debug.Log("다른 사람의 타워는 철거할 수 없습니다.");
        }
    }
    public override void Delete()
    {
        base.Delete();
        TowerContainer.Instance?.DeleteObj(this);
    }
    public void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PacketData d = new PacketData();
            SavePacket(d);
            d.Save();
            IPacketer.SendPacket(photonView,d);
        }
    }

    public void SavePacket(PacketData data)
    {
        AIData.SavePacket(data);
    }

    public void LoadPacket(PacketData data)
    {
        AIData.LoadPacket(data);
    }

    public float Angle
    {
        get
        {
            return mainTransform.eulerAngles.z;
        }
        set
        {
            mainTransform.eulerAngles = new Vector3(0,0,value);
        }
    }
    [SerializeField]private Transform mainTransform;
    public ITowerSO Data{get;private set;}
    public TowerAI AIData{get;private set;}
    public string Owner{get;private set;}
}