using UnityEngine.UIElements;
using System;

public class GameOverScreen : BaseScreen
{
    public event Action OnRestart;
    public event Action OnMainMenu;

    private Button restartBtn;
    private Button mainMenuBtn;

    public GameOverScreen(VisualElement root) : base(root) { }

    protected override void RegisterCallbacks()
    {
        restartBtn = root.Q<Button>("RestartButton");
        mainMenuBtn = root.Q<Button>("MainMenu");

        
        if (restartBtn != null) restartBtn.clicked += () => OnRestart?.Invoke();
        if (mainMenuBtn != null) mainMenuBtn.clicked += () => OnMainMenu?.Invoke();
    }

    protected override void OnShow()
    {
        restartBtn.Focus();
    }
}