using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Melee Weapon")]
public class MeleeWeaponData : ScriptableObject
{
    public AnimatorOverrideController controler;
    public int baseDamage = 10;
    public float comboWindowMultiplier = 1f;
    public int comboMaxBonus = 0;

    public void ApplyOverride(Animator Anim) 
    {
        Anim.runtimeAnimatorController = controler;
    }
}