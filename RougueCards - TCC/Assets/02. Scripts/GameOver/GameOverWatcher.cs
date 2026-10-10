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
        // Só quem tem a tag "Player" entra na contagem (inimigos ficam de fora)
        if (!health.CompareTag("Player"))
        {
            return;
        }

        registeredHealths.Add(health);

        // Quando morre de vez
        health.OnDeath += CheckGameOver;

        // Quando entra no estado abatido
        var downed = health.GetComponent<DownedState>();
        if (downed != null)
        {
            downed.OnDowned += CheckGameOver;
        }
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
        Debug.Log($"[GameOver] Checando. Registrados: {registeredHealths.Count}");
        if (gameOverTriggered || registeredHealths.Count == 0)
        {
            return;
        }

        // Se algum jogador ainda está de pé, não acaba
        foreach (var health in registeredHealths)
        {
            if (health != null && !health._isDead)
            {
                return;
            }
        }

        gameOverTriggered = true;
        uiController.ShowGameOver(BuildPlayersStats());
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
