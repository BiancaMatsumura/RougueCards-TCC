using RougueCards.Combo;
using System.Collections.Generic;
using UnityEngine;

namespace RougueCards.Attributes
{
    /// <summary>
    /// O Maestro orquestra a comunicação entre os dois jogadores.
    /// Aplica upgrades de cartas para ambos e gerencia boosts de combo.
    /// </summary>
    public class AttributeMaestro : MonoBehaviour
    {
        public static AttributeMaestro Instance;

        [Header("Referências dos Jogadores")]
        public PlayerStats player1;
        public PlayerStats player2;

        [Header("Sistema de Sinergia")]
        public ComboDatabase comboDatabase;
        private List<ComboData> activeCombos = new List<ComboData>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        /// <summary>
        /// Regra da Imagem: Aplica o upgrade da carta escolhida para ambos os jogadores (Efeito Compartilhado).
        /// </summary>
        public void ApplySharedUpgrade(StatType type, float value, bool isPercentage)
        {
            if (player1 != null)
            {
                var stat = player1.GetStat(type);
                stat?.AddModifier(value, isPercentage);

                // Se o upgrade for de vida, avisa o componente Health
                if (type == StatType.MaxHP)
                {
                    player1.GetComponent<Health>().RefreshMaxHP();
                }
            }

            if (player2 != null)
            {
                var stat = player2.GetStat(type);
                stat?.AddModifier(value, isPercentage);

                if (type == StatType.MaxHP)
                {
                    player2.GetComponent<Health>().RefreshMaxHP();
                }
            }

            // Se o upgrade for de Tamanho, avisa o Controller
            if (type == StatType.Size)
            {
                player1?.GetComponent<PlayerController>().RefreshSize();
                player2?.GetComponent<PlayerController>().RefreshSize();
            }

            Debug.Log($"Upgrade de {type} aplicado! Novo valor P1: {player1.GetStat(type).Value}");
        }

        /// <summary>
        /// Regra da Imagem: Determina quem tem o direito de escolha baseado no maior COMBO atual.
        /// </summary>
        public PlayerStats GetDecidingPlayer()
        {
            // Se um dos jogadores for nulo, retorna o outro
            if (player1 == null) return player2;
            if (player2 == null) return player1;

            // Quem tem o maior combo decide. Em caso de empate, Player 1 decide.
            if (player1.currentCombo >= player2.currentCombo)
            {
                return player1;
            }
            else
            {
                return player2;
            }
        }

        /// <summary>
        /// Regra da Imagem: Quando um jogador entra em combo, dá um bônus automático para o amigo.
        /// </summary>
        public void OnPlayerComboStarted(int comboOwnerID)
        {
            PlayerStats friend = (comboOwnerID == 1) ? player2 : player1;

            if (friend != null)
            {
                friend.stats.Damage.AddModifier(0.1f, true);
            }
        }

        /// <summary>
        /// Checa se algum combo foi formado após pegar uma nova carta.
        /// </summary>
        public void CheckForCardCombos()
        {
            if (comboDatabase == null) return;

            // Criamos uma lista com TODAS as cartas da dupla (Sinergia Cooperativa)
            List<CardData> combinedCards = new List<CardData>();
            if (player1 != null) combinedCards.AddRange(player1.inventoryCards);
            if (player2 != null) combinedCards.AddRange(player2.inventoryCards);

            foreach (var combo in comboDatabase.allPossibleCombos)
            {
                // Se já estiver ativo, pula (para não acumular o mesmo combo)
                if (activeCombos.Contains(combo)) continue;

                if (combo.IsSatisifed(combinedCards))
                {
                    ActivateCombo(combo);
                }
            }
        }

        private void ActivateCombo(ComboData combo)
        {
            activeCombos.Add(combo);
            Debug.Log($"<color=yellow>COMBO ATIVADO: {combo.comboName}!</color>");

            // Aplica o upgrade do combo
            ApplySharedUpgrade(combo.statToUpgrade, combo.upgradeValue, combo.isPercentage);

            // Se o combo tiver tempo limite, inicia a remoção
            if (combo.comboDuration > 0)
            {
                StartCoroutine(RemoveComboAfterTime(combo));
            }
        }

        private System.Collections.IEnumerator RemoveComboAfterTime(ComboData combo)
        {
            yield return new WaitForSecondsRealtime(combo.comboDuration);

            // Remove o bônus (aplica o valor negativo)
            ApplySharedUpgrade(combo.statToUpgrade, -combo.upgradeValue, combo.isPercentage);
            activeCombos.Remove(combo);

            Debug.Log($"Combo expirado: {combo.comboName}");
        }
    }
}
