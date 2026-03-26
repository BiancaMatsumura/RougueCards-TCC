using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public Sprite frontImage;
    public Sprite backImage;

    [Header("Progressão")]
    public int xpRequired;
    
}