using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgress", menuName = "Cards/PlayerProgress")]
public class PlayerProgress : ScriptableObject
{
    public int currentXP;
    public int currentStage = 0;

    [Tooltip("XP necessário para completar cada estágio")]
    public int[] xpPerStage = { 100, 150, 200 };

    public int MaxXP => currentStage < xpPerStage.Length ? xpPerStage[currentStage] : xpPerStage[^1];

    public event System.Action<int, int> OnXPChanged;  // current, max
    public event System.Action<int> OnStageCompleted;  // stage completado

    public void AddXP(int amount)
    {
        currentXP = Mathf.Min(currentXP + amount, MaxXP);
        Debug.Log($"XP: {currentXP}/{MaxXP} | Estágio: {currentStage}");
        OnXPChanged?.Invoke(currentXP, MaxXP);

        if (currentXP >= MaxXP)
            CompleteStage();
    }

    private void CompleteStage()
    {
        OnStageCompleted?.Invoke(currentStage);
        currentStage++;
        currentXP = 0;
        OnXPChanged?.Invoke(currentXP, MaxXP);
    }

    public void ResetXP()
    {
        currentXP = 0;
        currentStage = 0;
        OnXPChanged?.Invoke(currentXP, MaxXP);
    }
}