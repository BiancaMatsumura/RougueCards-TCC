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
        if (!other.CompareTag("Player")) return;

        if (IsAutoShooter)
        {
            autoshooter = other.GetComponent<AutoShooter>();

            if (autoshooter != null && rangedweapondata != null)
            {
                autoshooter.AddWeapon(rangedweapondata);
            }
        }
        else
        {
            shooter = other.GetComponent<Shooter>();

            if (shooter != null && meleeweapondata != null)
            {
                shooter.weaponData = meleeweapondata;
            }
        }
    }
}