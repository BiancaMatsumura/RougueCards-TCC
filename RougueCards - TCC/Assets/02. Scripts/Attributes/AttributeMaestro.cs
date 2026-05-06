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
            }

            if (player2 != null)
            {
                var stat = player2.GetStat(type);
                stat?.AddModifier(value, isPercentage);
            }
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
    }
}
