using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using RougueCards.GameOver;

public class GameOverScreen : BaseScreen
{
    public event Action OnRestart;
    public event Action OnMainMenu;

    private Button restartBtn;
    private Button mainMenuBtn;
    private VisualElement statsTable;

    public GameOverScreen(VisualElement root) : base(root) { }

    protected override void RegisterCallbacks()
    {
        restartBtn = root.Q<Button>("RestartButton");
        mainMenuBtn = root.Q<Button>("MainMenu");
        statsTable = root.Q<VisualElement>("StatsTable");


        if (restartBtn != null)
        {
            restartBtn.clicked += () => OnRestart?.Invoke();
        }

        if (mainMenuBtn != null)
        {
            mainMenuBtn.clicked += () => OnMainMenu?.Invoke();
        }
    }

    protected override void OnShow()
    {
        restartBtn.Focus();
    }

    /// <summary> Monta dinamicamente uma coluna de estatísticas por jogador. </summary>
    public void SetPlayerStats(List<GameOverPlayerStats> playersStats)
    {
        if (statsTable == null)
        {
            return;
        }

        statsTable.Clear();

        if (playersStats == null)
        {
            return;
        }

        foreach (var player in playersStats)
        {
            var column = new VisualElement();
            column.AddToClassList("gameover-stats-column");

            var nameLabel = new Label(player.PlayerName);
            nameLabel.AddToClassList("gameover-stats-title");
            column.Add(nameLabel);

            foreach (var stat in player.Stats)
            {
                var row = new VisualElement();
                row.AddToClassList("gameover-stats-row");

                var label = new Label(stat.Label);
                label.AddToClassList("gameover-stats-label");

                var value = new Label(stat.Value);
                value.AddToClassList("gameover-stats-value");

                row.Add(label);
                row.Add(value);
                column.Add(row);
            }

            statsTable.Add(column);
        }
    }
}
