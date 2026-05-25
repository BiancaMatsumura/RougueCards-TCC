using RougueCards.Combo;
using System.Collections.Generic;
using UnityEngine;

namespace RougueCards.Attributes
{
    /// <summary>
    /// O Maestro orquestra a comunicação entre os dois jogadores.
    /// É responsável por aplicar upgrades compartilhados, gerenciar a prioridade de escolha de cartas
    /// e processar as sinergias (combos) entre as cartas coletadas pela dupla.
    /// </summary>
    public class AttributeMaestro : MonoBehaviour
    {
        /// <summary> Instância estática para acesso global (Padrão Singleton). </summary>
        public static AttributeMaestro Instance;

        [Header("Referências dos Jogadores")]
        /// <summary> Referência aos atributos e estado do Jogador 1. </summary>
        public PlayerStats player1;

        /// <summary> Referência aos atributos e estado do Jogador 2. </summary>
        public PlayerStats player2;

        [Header("Sistema de Sinergia")]
        /// <summary> Banco de dados contendo todas as combinações de cartas que geram bônus especiais. </summary>
        public ComboDatabase comboDatabase;

        /// <summary> Lista interna de combos que estão atualmente ativos para evitar duplicatas. </summary>
        private List<ComboData> activeCombos = new List<ComboData>();

        /// <summary>
        /// Configura a instância Singleton no início da cena.
        /// </summary>
        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        /// <summary>
        /// Aplica um upgrade de atributo para ambos os jogadores simultaneamente (Regra de Efeito Compartilhado).
        /// Notifica componentes específicos (Health, PlayerController) caso o atributo exija atualização visual ou física.
        /// </summary>
        /// <param name="type">O tipo de atributo a ser modificado.</param>
        /// <param name="value">O valor do bônus ou penalidade.</param>
        /// <param name="isPercentage">Define se o valor é um multiplicador (true) ou um bônus fixo (false).</param>
        public void ApplySharedUpgrade(StatType type, float value, bool isPercentage)
        {
            if (player1 != null)
            {
                var stat = player1.GetStat(type);
                stat?.AddModifier(value, isPercentage);

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

            if (type == StatType.Size)
            {
                player1?.GetComponent<PlayerController>().RefreshSize();
                player2?.GetComponent<PlayerController>().RefreshSize();
            }

            Debug.Log($"[Maestro] Upgrade de {type} aplicado! Valor P1: {player1.GetStat(type).Value}");
        }

        /// <summary>
        /// Determina qual jogador tem o direito de escolha no painel de cartas.
        /// A decisão é baseada em quem possui o maior combo de eliminações atual.
        /// </summary>
        /// <returns>A instância de PlayerStats do jogador decisor.</returns>
        public PlayerStats GetDecidingPlayer()
        {
            if (player1 == null) return player2;
            if (player2 == null) return player1;

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
        /// Acionado quando um jogador atinge o estado de combo (múltiplas mortes rápidas).
        /// Concede um bônus de dano temporário ao parceiro (Sinergia de Combate).
        /// </summary>
        /// <param name="comboOwnerID">O ID do jogador que iniciou o combo.</param>
        public void OnPlayerComboStarted(int comboOwnerID)
        {
            PlayerStats friend = (comboOwnerID == 1) ? player2 : player1;

            if (friend != null)
            {
                StartCoroutine(TemporaryKillBoost(friend));
            }
        }

        /// <summary>
        /// Corrotina que gerencia o bônus de dano temporário concedido por mortes em sequência.
        /// Garante que o bônus seja removido após o tempo de expiração.
        /// </summary>
        /// <param name="target">O jogador que receberá o boost.</param>
        private IEnumerator<WaitForSecondsRealtime> TemporaryKillBoost(PlayerStats target)
        {
            float boostValue = 0.1f; // +10% de Dano
            target.stats.Damage.AddModifier(boostValue, true);
            Debug.Log($"[Combo] Player {target.playerID} recebeu boost de dano por ação do parceiro!");

            yield return new WaitForSecondsRealtime(2.0f);

            target.stats.Damage.AddModifier(-boostValue, true);
            Debug.Log($"[Combo] Boost de dano do Player {target.playerID} expirou.");
        }

        /// <summary>
        /// Varre o banco de dados de sinergias para verificar se a combinação de cartas atual da dupla
        /// desbloqueia algum combo de ScriptableObject.
        /// </summary>
        public void CheckForCardCombos()
        {
            if (comboDatabase == null) return;

            List<CardData> combinedCards = new List<CardData>();
            if (player1 != null) combinedCards.AddRange(player1.inventoryCards);
            if (player2 != null) combinedCards.AddRange(player2.inventoryCards);

            foreach (var combo in comboDatabase.allPossibleCombos)
            {
                if (activeCombos.Contains(combo)) continue;

                if (combo.IsSatisifed(combinedCards))
                {
                    ActivateCombo(combo);
                }
            }
        }

        /// <summary>
        /// Ativa um combo de sinergia de cartas, aplicando o bônus e iniciando o temporizador se necessário.
        /// </summary>
        /// <param name="combo">Os dados do combo a ser ativado.</param>
        private void ActivateCombo(ComboData combo)
        {
            activeCombos.Add(combo);
            Debug.Log($"<color=yellow>[Sinergia] COMBO ATIVADO: {combo.comboName}!</color>");

            ApplySharedUpgrade(combo.statToUpgrade, combo.upgradeValue, combo.isPercentage);

            if (combo.comboDuration > 0)
            {
                StartCoroutine(RemoveComboAfterTime(combo));
            }
        }

        /// <summary>
        /// Corrotina que remove os bônus de um combo de sinergia após o tempo definido no ScriptableObject.
        /// </summary>
        /// <param name="combo">O combo que irá expirar.</param>
        private System.Collections.IEnumerator RemoveComboAfterTime(ComboData combo)
        {
            yield return new WaitForSecondsRealtime(combo.comboDuration);

            // Remove o bônus aplicando o valor negativo correspondente
            ApplySharedUpgrade(combo.statToUpgrade, -combo.upgradeValue, combo.isPercentage);
            activeCombos.Remove(combo);

            Debug.Log($"[Sinergia] Combo expirado: {combo.comboName}");
        }
    }
}