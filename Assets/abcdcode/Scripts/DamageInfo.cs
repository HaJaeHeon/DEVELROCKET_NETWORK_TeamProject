public class DamageInfo : IDamageInfo
{
    public DamageInfo(float dmg, string owner)
    {
        Dmg = dmg;
        Owner =owner;
    }

    public float Dmg {get;set;}

    public string Owner {get;set;}
}