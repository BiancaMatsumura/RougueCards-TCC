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
        restartBtn  = root.Q<Button>("RestartButton");
        mainMenuBtn = root.Q<Button>("MainMenu");

        restartBtn.RegisterCallback<NavigationSubmitEvent>(_ => OnRestart?.Invoke());
        mainMenuBtn.RegisterCallback<NavigationSubmitEvent>(_ => OnMainMenu?.Invoke());
    }

    protected override void OnShow()
    {
        restartBtn.Focus();
    }
}