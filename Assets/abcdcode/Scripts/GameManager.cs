using Photon.Pun;
using UnityEngine;
public class GameManager : SingletonBehavior<GameManager>
{
    public override void Awake()
    {
        base.Awake();
        if(isOffDebug)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
        }
    }
    [SerializeField]private bool isOffDebug;
}