using UnityEngine;
public class DebugItemTrade : MonoBehaviour
{
    public bool IsAutoShooter = true;
    public RangedWeaponData rangedweapondata;
    public MeleeWeaponData meleeweapondata;
    private AutoShooter autoshooter;
    private Shooter shooter;

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && IsAutoShooter)
        {
            autoshooter = other.GetComponent<AutoShooter>();
            autoshooter.weaponData = rangedweapondata;
        }
        else if (other.CompareTag("Player")) 
        { 
            shooter = other.GetComponent<Shooter>();
            shooter.weaponData = meleeweapondata;
        }
    }
}
