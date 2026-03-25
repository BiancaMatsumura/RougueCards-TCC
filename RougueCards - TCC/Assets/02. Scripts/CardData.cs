using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/CardData")]
public class CardData : ScriptableObject
{
    public string cardId;
    public Sprite frontImage;
    public Sprite backImage;
    
}