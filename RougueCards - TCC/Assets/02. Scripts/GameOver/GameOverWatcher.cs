using System.Collections.Generic;
using RougueCards.GameOver;
using UnityEngine;

public class GameOverWatcher : MonoBehaviour
{
    [SerializeField] private UI_Controller uiController;

    private readonly List<Health> registeredHealths = new();

    private bool player1Dead = false;
    private bool player2Dead = false;
    private bool gameOverTriggered = false;

    public static GameOverWatcher Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Chamado por cada Health ao nascer
    public void RegisterPlayer(Health health)
    {
        registeredHealths.Add(health);
        health.OnDeath += () => HandleDeath(health.playerID);
    }

    private void HandleDeath(int playerID)
    {
        if (playerID == 1)
        {
            player1Dead = true;
        }

        if (playerID == 2)
        {
            player2Dead = true;
        }

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (gameOverTriggered)
        {
            return;
        }

        bool soloMode = registeredHealths.Count == 1;

        if (soloMode || (player1Dead && player2Dead))
        {
            gameOverTriggered = true;
            uiController.ShowGameOver(BuildPlayersStats());
        }
    }

    /// <summary> Reúne as estatísticas de cada jogador registrado para exibir no Game Over. </summary>
    private List<GameOverPlayerStats> BuildPlayersStats()
    {
        var result = new List<GameOverPlayerStats>();

        foreach (var health in registeredHealths)
        {
            if (health == null || health.pStats == null)
            {
                continue;
            }
            result.Add(GameOverStatsBuilder.Build(health.pStats));
        }

        return result;
    }
}
