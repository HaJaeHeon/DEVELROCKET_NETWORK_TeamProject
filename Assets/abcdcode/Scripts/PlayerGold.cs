using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// 플레이어 재화(골드). Player CustomProperties의 "Gold"(float)에 저장되어 모든 클라이언트에 동기화
/// 값 변경은 해당 플레이어 본인 클라이언트에서만 수행
/// </summary>
public static class PlayerGold
{
    public const string Key = "Gold";
    public const float StartGold = 30f;

    public static float Get(Player player)
    {
        if(player != null && player.CustomProperties.TryGetValue(Key, out object value) && value is float gold)
        {
            return gold;
        }
        return 0f;
    }
    public static float Local => Get(PhotonNetwork.LocalPlayer);

    public static void Set(float gold)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { Key, gold } });
    }
    public static void Add(float amount)
    {
        Set(Local + amount);
    }
    /// <summary>
    /// 골드가 충분하면 차감 후 true
    /// </summary>
    public static bool TrySpend(float amount)
    {
        if(Local < amount) return false;
        Set(Local - amount);
        return true;
    }
}
