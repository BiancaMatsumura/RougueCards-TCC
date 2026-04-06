using UnityEngine;
using RougueCards.Attributes;

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public Sprite frontImage;
    public Sprite backImage;

    [Header("Progressão")]
    public int xpRequired;

    [Header("Efeito de Upgrade")]

    /// <summary> Qual atributo esta carta melhora? </summary>
    public StatType statToUpgrade;

    /// <summary> Quanto ela melhora? </summary>
    public float upgradeValue;

    /// <summary> É um bônus percentual (multiplicador)? </summary>
    public bool isPercentage;
}