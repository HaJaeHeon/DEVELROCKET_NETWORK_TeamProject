using Photon.Realtime;

public interface IReward : ISOData
{
    public void OnEarned();
    public void OnDieEnemy(IEnemy enemy, Player killer);
    public float DamageMult(ref IDamageInfo dmg);
    public bool CheckCondition();
    public Player Owner{get;}
}