using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgress", menuName = "Cards/PlayerProgress")]
public class PlayerProgress : ScriptableObject
{
    public int currentXP;

    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"XP compartilhado: {currentXP}");
    }
}