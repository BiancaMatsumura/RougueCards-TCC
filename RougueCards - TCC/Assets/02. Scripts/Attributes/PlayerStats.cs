using UnityEngine;

namespace RougueCards.Attributes
{
    /// <summary>
    /// Este é um script que deve ser anexado ao Prefab de Jogador.
    /// Ele gerencia a ficha de atributos (StatSheet) e o sistema de Combo.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Configuração")]
        /// <summary> Identificador para saber se é o Jogador 1 ou 2. </summary>
        public int playerID;

        /// <summary> Instância da sua tabela de atributos. </summary>
        public StatSheet stats = new();

        [Header("Sistema de Combo (Regra da Imagem)")]
        /// <summary> Quantos inimigos o jogador eliminou em sequência. </summary>
        public int currentCombo = 0;

        /// <summary> Tempo restante antes do combo resetar. </summary>
        private float comboTimer = 0;

        /// <summary> Tempo de tolerância entre abates (ex: 2 segundos). </summary>
        [SerializeField] private float comboDuration = 2.0f;

        /// <summary>
        /// Deve ser chamado sempre que o jogador matar um inimigo.
        /// Aumenta o combo e notifica o Maestro se atingir o estado de COMBO (2+).
        /// </summary>
        public void AddKill()
        {
            currentCombo++;
            comboTimer = comboDuration;

            // Regra da Imagem: Se matar 2 ou mais, entra em estado de COMBO.
            if (currentCombo >= 2)
            {
                // Notifica o Maestro para dar o boost pro amigo
                if (AttributeMaestro.Instance != null)
                {
                    AttributeMaestro.Instance.OnPlayerComboStarted(playerID);
                }
            }
        }

        private void Update()
        {
            // Lógica de decaimento do tempo do combo
            if (comboTimer > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0)
                {
                    currentCombo = 0;
                }
            }
        }

        /// <summary>
        /// Método auxiliar para encontrar um atributo específico dentro da sua StatSheet 
        /// usando o Enum StatType.
        /// </summary>
        public PlayerAttributes GetStat(StatType type)
        {
            return type switch
            {
                StatType.MaxHP => stats.MaxHP,
                StatType.Damage => stats.Damage,
                StatType.AttackSpeed => stats.AttackSpeed,
                StatType.MoveSpeed => stats.MoveSpeed,
                StatType.Luck => stats.Luck,
                StatType.CritChance => stats.CritChance,
                StatType.LifeSteal => stats.LifeSteal,
                StatType.Thorns => stats.Thorns,
                StatType.CollectionRange => stats.CollectionRange,
                // Adicione os outros atributos da StatSheet aqui conforme precisar
                _ => null
            };
        }
    }
}
