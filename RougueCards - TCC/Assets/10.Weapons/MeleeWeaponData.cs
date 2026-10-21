using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Melee Weapon")]
public class MeleeWeaponData : ScriptableObject
{
    public AnimatorOverrideController controler;
    public int baseDamage = 10;
    public float comboWindowMultiplier = 1f;
    public int comboMax = 0;
    public GameObject WeaponPrefab;

    public void ApplyOverride(Animator Anim) 
    {
        if(controler != null) 
        {
            Anim.runtimeAnimatorController = controler;
        }
        else { Debug.LogWarning("N�o existe animator controler, ser� usado o padr�o"); }
        
    }
}