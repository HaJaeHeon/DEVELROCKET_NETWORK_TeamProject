

using Photon.Pun;

public static class DifficultyManager
{
    public static float EnemyHPMult()
    {
        switch(CurPlayerCount)
        {
            case 1 :
                return 1;
            case 2 : 
                return 1.5f;
            case 3 :
                return 2f;
            default :
                return 1;
        }
    }
    public static int CurPlayerCount => PhotonNetwork.CurrentRoom == null ? 1 : PhotonNetwork.CurrentRoom.PlayerCount;
}