using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Cards/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    public CardData[] allCards;

    public CardData[] GetAvailableCards(PlayerProgress progress, int maxCards = 3, bool randomize = true)
    {
        var available = allCards
            .Where(card => !card.bossOnly && card.xpRequired <= progress.currentXP);

        if (randomize)
            available = available.OrderBy(_ => Random.value);

        return available.Take(maxCards).ToArray();
    }

    public CardData[] GetBossCards(CardData[] pool, int maxCards = 3)
    {
        return pool
            .Where(c => c != null)
            .OrderBy(_ => Random.value)
            .Take(maxCards)
            .ToArray();
    }
}