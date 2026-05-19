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

    [Header("Tipo de Efeito")]
    public CardEffectType effectType;

    [Header("Stat Upgrade")]
    public StatType statToUpgrade;
    public float upgradeValue;
    public bool isPercentage;

    [Header("Weapon Unlock")]
    public RangedWeaponData rangedWeapon;
    public MeleeWeaponData meleeWeapon;
}