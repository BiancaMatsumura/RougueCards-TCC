using UnityEngine;
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

    private class PlayerPanel
    {
        public VisualElement Root;
        public Label NameLabel;
        public VisualElement TopBadge;
        public VisualElement StatsContainer;
    }

    private PlayerPanel[] panels;

    public GameOverScreen(VisualElement root) : base(root) { }

    protected override void RegisterCallbacks()
    {
        restartBtn = root.Q<Button>("RestartButton");
        mainMenuBtn = root.Q<Button>("MainMenu");

        panels = new[]
        {
            BuildPanel("P1Panel"),
            BuildPanel("P2Panel")
        };

        if (restartBtn != null)
        {
            restartBtn.clicked += () => OnRestart?.Invoke();
        }

        if (mainMenuBtn != null)
        {
            mainMenuBtn.clicked += () => OnMainMenu?.Invoke();
        }
    }

    private PlayerPanel BuildPanel(string panelName)
    {
        var panelRoot = root.Q<VisualElement>(panelName);
        if (panelRoot == null)
        {
            Debug.LogWarning($"[GameOverScreen] Painel '{panelName}' não encontrado no UXML.");
            return null;
        }

        return new PlayerPanel
        {
            Root = panelRoot,
            NameLabel = panelRoot.Q<Label>("NameLabel"),
            TopBadge = panelRoot.Q<VisualElement>("TopBadgeLabel"),
            StatsContainer = panelRoot.Q<VisualElement>("StatsContainer")
        };
    }

    protected override void OnShow()
    {
        restartBtn.Focus();
    }

    /// <summary> Preenche os painéis fixos de cada jogador com as estatísticas. </summary>
    public void SetPlayerStats(List<GameOverPlayerStats> playersStats)
    {
        // Esconde todos os painéis; só os que tiverem jogador voltam a aparecer
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.Root.style.display = DisplayStyle.None;
            }
        }

        if (playersStats == null)
        {
            return;
        }

        for (int i = 0; i < playersStats.Count && i < panels.Length; i++)
        {
            FillPanel(panels[i], playersStats[i]);
        }
    }

    private void FillPanel(PlayerPanel panel, GameOverPlayerStats player)
    {
        if (panel == null)
        {
            return;
        }

        panel.Root.style.display = DisplayStyle.Flex;

        if (panel.NameLabel != null)
        {
            panel.NameLabel.text = player.PlayerName;
        }

        if (panel.TopBadge != null)
        {
            panel.TopBadge.style.visibility = player.IsTopPlayer ? Visibility.Visible : Visibility.Hidden;
        }

        if (panel.StatsContainer == null)
        {
            return;
        }

        panel.StatsContainer.Clear();

        foreach (var stat in player.Stats)
        {
            var row = new VisualElement();
            row.AddToClassList("gameover-stats-row");

            var label = new Label(stat.Label);
            label.AddToClassList("gameover-stats-label");

            var value = new Label(stat.Value);
            value.AddToClassList("gameover-stats-value");
            if (player.IsTopPlayer)
            {
                value.AddToClassList("gameover-stats-value--top");
            }

            row.Add(label);
            row.Add(value);
            panel.StatsContainer.Add(row);
        }
    }
}