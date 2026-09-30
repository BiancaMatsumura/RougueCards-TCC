using RougueCards.Combo;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        /// <summary> Referência ao PlayerInput do Jogador 1, usada para bloquear/restaurar seus controles. </summary>
        public PlayerInput player1Input;

        /// <summary> Referência ao PlayerInput do Jogador 2, usada para bloquear/restaurar seus controles. </summary>
        public PlayerInput player2Input;

        /// <summary> Último jogador que eliminou um inimigo especial. Enquanto definido, ele tem prioridade de escolha sobre a regra de combo. </summary>
        private PlayerStats lastSpecialEnemyKiller;

        /// <summary> Jogador que teve o input bloqueado durante a escolha exclusiva de carta (null se ninguém estiver bloqueado). </summary>
        private PlayerStats lockedOutPlayer;

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
            if (Instance == null)
            {
                Instance = this;
            }
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
        /// Se um inimigo especial já foi eliminado, quem deu o golpe final nele tem prioridade.
        /// Caso contrário, a decisão é baseada em quem possui o maior combo de eliminações atual.
        /// </summary>
        /// <returns>A instância de PlayerStats do jogador decisor.</returns>
        public PlayerStats GetDecidingPlayer()
        {
            if (lastSpecialEnemyKiller != null)
            {
                return lastSpecialEnemyKiller;
            }

            if (player1 == null)
            {
                return player2;
            }

            if (player2 == null)
            {
                return player1;
            }

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
        /// Registra a referência de PlayerInput de um jogador, permitindo que o Maestro
        /// bloqueie e restaure seus controles posteriormente (ex: durante a escolha exclusiva de carta).
        /// </summary>
        /// <param name="playerID">1 ou 2.</param>
        /// <param name="input">O componente PlayerInput da instância do jogador.</param>
        public void RegisterPlayerInput(int playerID, PlayerInput input)
        {
            if (playerID == 1)
            {
                player1Input = input;
            }
            else if (playerID == 2)
            {
                player2Input = input;
            }
        }

        /// <summary>
        /// Chamado quando um inimigo especial é eliminado. O jogador que deu o golpe final
        /// passa a ter prioridade de escolha na próxima liberação de upgrade.
        /// </summary>
        /// <param name="killer">O jogador que eliminou o inimigo especial.</param>
        public void RegisterSpecialEnemyKill(PlayerStats killer)
        {
            lastSpecialEnemyKiller = killer;
            Debug.Log($"[Maestro] Inimigo especial eliminado por Player {killer.playerID}. Ele decidirá a próxima carta.");
        }

        /// <summary>
        /// Bloqueia o input do jogador que NÃO tem prioridade de escolha (ver <see cref="GetDecidingPlayer"/>),
        /// permitindo que apenas o jogador decisor consiga escolher a carta.
        /// </summary>
        public void LockOutNonDecidingPlayer()
        {
            PlayerStats decider = GetDecidingPlayer();
            if (decider == null)
            {
                return;
            }

            lockedOutPlayer = (decider == player1) ? player2 : player1;
            if (lockedOutPlayer == null)
            {
                return;
            }

            PlayerInput input = (lockedOutPlayer == player1) ? player1Input : player2Input;
            if (input != null && input.inputIsActive)
            {
                input.DeactivateInput();
                Debug.Log($"[Maestro] Input do Player {lockedOutPlayer.playerID} bloqueado durante a escolha de carta.");
            }
        }

        /// <summary>
        /// Restaura o input do jogador que foi bloqueado em <see cref="LockOutNonDecidingPlayer"/>.
        /// </summary>
        public void RestoreLockedPlayerInput()
        {
            if (lockedOutPlayer == null)
            {
                return;
            }

            PlayerInput input = (lockedOutPlayer == player1) ? player1Input : player2Input;
            if (input != null && !input.inputIsActive)
            {
                input.ActivateInput();
                Debug.Log($"[Maestro] Input do Player {lockedOutPlayer.playerID} restaurado.");
            }

            lockedOutPlayer = null;
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
            if (comboDatabase == null)
            {
                return;
            }

            List<CardData> combinedCards = new List<CardData>();
            if (player1 != null)
            {
                combinedCards.AddRange(player1.inventoryCards);
            }

            if (player2 != null)
            {
                combinedCards.AddRange(player2.inventoryCards);
            }

            foreach (var combo in comboDatabase.allPossibleCombos)
            {
                if (activeCombos.Contains(combo))
                {
                    continue;
                }

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