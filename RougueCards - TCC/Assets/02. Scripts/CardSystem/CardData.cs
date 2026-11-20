using UnityEngine;
using RougueCards.Attributes;

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite cardImage;

    [Header("Progressão")]
    public int xpRequired;
    public bool bossOnly;

    [Header("Tipo de Efeito")]
    public CardEffectType effectType;

    [Header("Stat Upgrade")]
    public StatType statToUpgrade;
    public float upgradeValue;
    public bool isPercentage;

    [Header("Weapon Unlock")]
    public RangedWeaponData rangedWeapon;
    public MeleeWeaponData meleeWeapon;

    [Header("Instant Effect")]
    public InstantEffectType instantEffectType;
    public float instantValue;      // XP, quantidade de cura, etc.
    public float instantDuration;   // duração do freeze, por exemplo
}

public enum InstantEffectType
{
    None, HealAll, FreezeEnemies, KillAllEnemies, AddXP,
    DamageShare, OneHitKill, Shield
}