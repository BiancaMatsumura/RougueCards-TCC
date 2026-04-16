using UnityEngine.UIElements;
using System;

public class PauseScreen : BaseScreen
{
    public event Action OnResume;
    public event Action OnRestart;
    public event Action OnMainMenu;
    public event Action OnOptions;   // novo

    private Button resumeBtn;
    private Button restartBtn;
    private Button mainMenuBtn;
    private Button optionsBtn;

    public PauseScreen(VisualElement root) : base(root) { }

    protected override void RegisterCallbacks()
    {
        resumeBtn  = root.Q<Button>("ResumeButton");
        restartBtn = root.Q<Button>("RestartButton");
        mainMenuBtn = root.Q<Button>("MainMenu");
        optionsBtn = root.Q<Button>("OptionsButton");

        resumeBtn.RegisterCallback<NavigationSubmitEvent>(_ => OnResume?.Invoke());
        restartBtn.RegisterCallback<NavigationSubmitEvent>(_ => OnRestart?.Invoke());
        mainMenuBtn.RegisterCallback<NavigationSubmitEvent>(_ => OnMainMenu?.Invoke());
        optionsBtn?.RegisterCallback<NavigationSubmitEvent>(_ => OnOptions?.Invoke());
    }

    protected override void OnShow()
    {
        resumeBtn.Focus();
    }
}