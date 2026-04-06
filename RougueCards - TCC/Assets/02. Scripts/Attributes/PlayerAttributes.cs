using System;
using System.ComponentModel;
using UnityEngine;

namespace RougueCards.Attributes
{
    /// <summary>
    /// Classe base para um atributo individual. 
    /// Gerencia o valor base e os bônus aplicados por cartas ou combos.
    /// </summary>
    [Serializable]
    public class PlayerAttributes
    {
        public float BaseValue;
        public float additiveBonus = 0;
        public float multiplierBonus = 1;

        /// <summary> Valor final calculado do atributo. </summary>
        public float Value => (BaseValue + additiveBonus) * multiplierBonus;

        /// <summary> Adiciona um bônus permanente ou temporário ao atributo. </summary>
        public void AddModifier(float amount, bool isMultiplier)
        {
            if (isMultiplier)
            {
                additiveBonus += amount;
            }
            else
            { 
                additiveBonus += amount;
            }
        }
    }
}
