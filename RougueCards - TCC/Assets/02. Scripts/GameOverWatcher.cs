using UnityEngine;

public class GameOverWatcher : MonoBehaviour
{
    [SerializeField] private UI_Controller uiController;

    private bool player1Dead = false;
    private bool player2Dead = false;
    private bool gameOverTriggered = false;
    private int registeredPlayers = 0;

    public static GameOverWatcher Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Chamado por cada Health ao nascer
    public void RegisterPlayer(Health health)
    {
        registeredPlayers++;
        health.OnDeath += () => HandleDeath(health.playerID);
    }

    private void HandleDeath(int playerID)
    {
        if (playerID == 1) player1Dead = true;
        if (playerID == 2) player2Dead = true;

        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (gameOverTriggered) return;

        bool soloMode = registeredPlayers == 1;

        if (soloMode || (player1Dead && player2Dead))
        {
            gameOverTriggered = true;
            uiController.ShowGameOver();
        }
    }
}