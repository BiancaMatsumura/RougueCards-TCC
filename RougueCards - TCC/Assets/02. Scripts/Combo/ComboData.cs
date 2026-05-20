using UnityEngine;
using System.Collections.Generic;
using RougueCards.Attributes;

namespace RougueCards.Combo
{
    [CreateAssetMenu(fileName = "NewCombo", menuName = "Cards/Combo Synergy")]
    public class ComboData : ScriptableObject
    {
        public string comboName;

        [Tooltip("Lista de cartas necessárias para ativar este combo")]
        public List<CardData> requiredCards;

        [Header("Configurações de Tempo")]
        [Tooltip("Se o combo for temporário, quanto tempo ele dura? (0 = permanente enquanto tiver as cartas)")]
        public float comboDuration = 5f;

        [Header("Bônus do Combo")]
        public StatType statToUpgrade;
        public float upgradeValue;
        public bool isPercentage;

        /// <summary>
        /// Verifica se uma lista de cartas coletadas contém todas as cartas necessárias para este combo.
        /// </summary>
        public bool IsSatisifed(List<CardData> collectedCards)
        {
            foreach (var req in requiredCards)
            {
                if (!collectedCards.Contains(req)) return false;
            }
            return true;
        }
    }
}