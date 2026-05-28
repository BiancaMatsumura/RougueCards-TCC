using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RangedWeaponData weaponToGive;

    private void OnTriggerEnter(Collider other)
    {
        AutoShooter shooter = other.GetComponent<AutoShooter>();

        if (shooter != null && weaponToGive != null)
        {
            shooter.AddWeapon(weaponToGive);
            Destroy(gameObject);
        }
    }
}