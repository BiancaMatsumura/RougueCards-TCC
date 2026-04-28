using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RangedWeaponData weaponToGive;

    private void OnTriggerEnter(Collider other)
    {
        AutoShooter shooter = other.GetComponent<AutoShooter>();

        if (shooter != null)
        {
            shooter.SetWeapon(weaponToGive);
            Destroy(gameObject);
        }
    }
}