using System;
using System.Reflection;
using UnityEngine;

namespace RougueCards.Attributes
{
    [Serializable]
    /// <summary>
    /// Tabela completa de estatísticas baseada na interface do jogo.
    /// Centraliza todos os dados de sobrevivência, ataque, movimentação e economia.
    /// </summary>
    public class StatSheet
    {
        [Header("Sobrevivência")]
        public PlayerAttributes MaxHP = new() { BaseValue = 134 };
        public PlayerAttributes HPRegen = new() { BaseValue = 10 };
        public PlayerAttributes Overheal = new() { BaseValue = 0 };
        public PlayerAttributes Shield = new() { BaseValue = 0 };
        public PlayerAttributes Armor = new() { BaseValue = 0 };
        public PlayerAttributes Evasion = new() { BaseValue = 1 }; // 1%
        public PlayerAttributes LifeSteal = new() { BaseValue = 7 }; // 7%
        public PlayerAttributes Thorns = new() { BaseValue = 5 };

        [Header("Ataque")]
        public PlayerAttributes Damage = new() { BaseValue = 1.7f };
        public PlayerAttributes CritChance = new() { BaseValue = 1 }; // 1%
        public PlayerAttributes CritDamage = new() { BaseValue = 1.0f };
        public PlayerAttributes AttackSpeed = new() { BaseValue = 106 }; // 106%
        public PlayerAttributes ProjectileQty = new() { BaseValue = 0 };
        public PlayerAttributes Ricochet = new() { BaseValue = 0 };

        [Header("Movimentação e Outros")]
        public PlayerAttributes Size = new() { BaseValue = 1.0f };
        public PlayerAttributes ProjectileSpeed = new() { BaseValue = 1.2f };
        public PlayerAttributes Duration = new() { BaseValue = 1.0f };
        public PlayerAttributes EliteDamage = new() { BaseValue = 1.0f };
        public PlayerAttributes Knockback = new() { BaseValue = 1.0f };
        public PlayerAttributes MoveSpeed = new() { BaseValue = 1.0f };
        public PlayerAttributes ExtraJumps = new() { BaseValue = 0 };
        public PlayerAttributes JumpHeight = new() { BaseValue = 9 };
        public PlayerAttributes Luck = new() { BaseValue = 7 }; // 7%
        public PlayerAttributes Difficulty = new() { BaseValue = 32 }; // 32%

        [Header("Coleta e Economia")]
        public PlayerAttributes CollectionRange = new() { BaseValue = 5 };
        public PlayerAttributes XPMultiplier = new() { BaseValue = 1.0f };
        public PlayerAttributes GoldMultiplier = new() { BaseValue = 1.0f };
        public PlayerAttributes SilverMultiplier = new() { BaseValue = 1.0f };
        public PlayerAttributes EliteSpawnRate = new() { BaseValue = 1.2f };
        public PlayerAttributes PowerupMultiplier = new() { BaseValue = 1.0f };
        public PlayerAttributes PowerupChance = new() { BaseValue = 1.0f };

        /// <sumarry>
        /// Pecorre tofdos os campos PlayerAtributes desta classe e reseta os bônus.
        /// </sumarry>
        public void ResetAllAttributes()
        {
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(PlayerAttributes))
                {
                    var attr = (PlayerAttributes)field.GetValue(this);
                    attr?.ResetModifiers();
                }
            }
        }
    }
}
