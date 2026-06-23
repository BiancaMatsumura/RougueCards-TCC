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
        resumeBtn = root.Q<Button>("ResumeButton");
        restartBtn = root.Q<Button>("RestartButton");
        mainMenuBtn = root.Q<Button>("MainMenuButton");
        optionsBtn = root.Q<Button>("OptionsButton");

        
        if (resumeBtn != null) resumeBtn.clicked += () => OnResume?.Invoke();
        if (restartBtn != null) restartBtn.clicked += () => OnRestart?.Invoke();
        if (mainMenuBtn != null) mainMenuBtn.clicked += () => OnMainMenu?.Invoke();
        if (optionsBtn != null) optionsBtn.clicked += () => OnOptions?.Invoke();
    }

    protected override void OnShow()
    {
        resumeBtn.Focus();
    }
}