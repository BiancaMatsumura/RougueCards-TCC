using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Melee Weapon")]
public class MeleeWeaponData : ScriptableObject
{
    public int baseDamage = 10;
    public float comboWindowMultiplier = 1f;
    public int comboMaxBonus = 0;
}