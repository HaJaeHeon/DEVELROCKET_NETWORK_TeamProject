using ExitGames.Client.Photon;
using Photon.Pun;

/// <summary>
/// 플레이어 공용 체력. Room CustomProperties의 "PlayerHP"(int)에 저장되어 모든 클라이언트에 동기화
/// 값 변경은 마스터 클라이언트에서만 수행하며, 0이 되면 모든 클라이언트에서 GameManager.GameOver 호출
/// </summary>
public static class PlayerHP
{
    public const string Key = "PlayerHP";

    // 마스터 기준 값. Room 프로퍼티 반영은 서버 왕복 후라 같은 프레임 연속 감소 시 누락되지 않도록 별도로 보관
    private static int s_hp;

    public static int Get()
    {
        if(PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) return s_hp;
        var room = PhotonNetwork.CurrentRoom;
        if(room.CustomProperties.TryGetValue(Key, out object value) && value is int hp)
        {
            return hp;
        }
        return 0;
    }
    /// <summary>
    /// 게임 시작 시 체력 설정 (마스터 전용)
    /// </summary>
    public static void Init(int hp)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        s_hp = hp;
        Sync();
    }
    /// <summary>
    /// 체력 감소, 0이 되면 게임오버 패킷 전송 (마스터 전용)
    /// </summary>
    public static void Damage(int amount)
    {
        if(!PhotonNetwork.IsMasterClient || s_hp <= 0) return;
        s_hp = System.Math.Max(0, s_hp - amount);
        Sync();
        if(s_hp <= 0)
        {
            PacketData d = new PacketData();
            d.Write(GameManager.GameOverPacket);
            IPacketer.SendPacket(GameManager.Instance.MainGamePacketer.photonView, d);
        }
    }
    private static void Sync()
    {
        if(PhotonNetwork.CurrentRoom == null) return;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { Key, s_hp } });
    }
}
