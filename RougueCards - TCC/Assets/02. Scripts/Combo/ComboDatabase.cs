using RougueCards.Combo;
using System.Collections.Generic;
using UnityEngine;

namespace RougueCards.Combo
{
    [CreateAssetMenu(fileName = "ComboDatabase", menuName = "Cards/Combo Database")]
    public class ComboDatabase : ScriptableObject
    {
        public List<ComboData> allPossibleCombos;
    }
}