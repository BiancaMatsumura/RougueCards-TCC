using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using System.Collections;

public class UI_Controller : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private UIDocument optionsUIDocument;
    [SerializeField] private OptionsManager optionsManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private PlayerProgress playerProgress;
    public AudioSource clickSound;

    private InputAction pauseAction;
    private InputSystemUIInputModule inputModule;
    private bool isPaused = false;

    private PauseScreen pauseScreen;
    private GameOverScreen gameOverScreen;

    private bool optionsOpenedFromPause = false;

    private Button optionButton;
    private Button closeOptionsPanel;
    private VisualElement optionsPanel;

    private VisualElement howToPlayPanel;
    private Button closeHowToPlayPanel;
    private Button openHowToPlayButton;
    private VisualElement finalScreen;
    private Button finalRestartButton;
    private Button finalMainMenuButton;
    private Button finalQuitButton;

    void Awake()
    {
        // Garante a busca do Input no Awake para que o OnEnable/OnDisable funcione perfeitamente
        if (inputActions != null)
        {
            pauseAction = inputActions.FindAction("Pause", true);
        }
        else
        {
            Debug.LogError("InputActions não foi atribuído no UI_Controller!");
        }
    }

    void Start()
    {
        inputModule = FindFirstObjectByType<InputSystemUIInputModule>();

        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("UIDocument principal não encontrado neste GameObject!");
            return;
        }

        var root = uiDoc.rootVisualElement;

        var pauseVisual = root.Q<VisualElement>("PauseMenu");
        var gameOverVisual = root.Q<VisualElement>("GameOverMenu");

        var finalVisual = root.Q<VisualElement>("ToBeContinuedMenu");
        finalQuitButton = root.Q<Button>("finalQuitButton");
        finalRestartButton = root.Q<Button>("finalRestartButton");
        finalMainMenuButton = root.Q<Button>("finalMainMenuButton");

        if (finalRestartButton != null)
            finalRestartButton.clicked += HandleRestart;

        if (finalMainMenuButton != null)
            finalMainMenuButton.clicked += HandleMainMenu;

        if (finalQuitButton != null)
            finalQuitButton.clicked += QuitGame;

        if (pauseVisual != null)
        {
            pauseScreen = new PauseScreen(pauseVisual);
            pauseScreen.OnResume += HandleResume;
            pauseScreen.OnRestart += HandleRestart;
            pauseScreen.OnMainMenu += HandleMainMenu;
        }

        if (gameOverVisual != null)
        {
            gameOverScreen = new GameOverScreen(gameOverVisual);
            gameOverScreen.OnRestart += HandleRestart;
            gameOverScreen.OnMainMenu += HandleMainMenu;
        }

        // NOVO: guarda a referência e garante que começa escondida
        if (finalVisual != null)
        {
            finalScreen = finalVisual;
            finalScreen.style.display = DisplayStyle.None;
            finalScreen.pickingMode = PickingMode.Ignore;
        }

        optionButton = root.Q<Button>("optionsButton");
        if (optionButton != null)
        {
            optionButton.clicked += OpenOptionsMenu;
        }

        if (optionsUIDocument != null)
        {
            var optionsRoot = optionsUIDocument.rootVisualElement;
            optionsPanel = optionsRoot.Q<VisualElement>("OptionsPanel");
            closeOptionsPanel = optionsRoot.Q<Button>("closeButton");

            if (closeOptionsPanel != null)
            {
                closeOptionsPanel.clicked += CloseOptionsMenu;
            }
        }

        howToPlayPanel = root.Q<VisualElement>("HowToPlayMenu");
        openHowToPlayButton = root.Q<Button>("howToPlayButton");
        closeHowToPlayPanel = root.Q<Button>("closeHTP");

        if (openHowToPlayButton != null)
        {
            openHowToPlayButton.clicked += OpenHowToPlayPanel;
        }

        if (closeHowToPlayPanel != null)
        {
            closeHowToPlayPanel.clicked += CloseHowToPlayPanel;
        }
    }
    private void OpenOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.style.visibility = Visibility.Visible;
            optionsPanel.pickingMode = PickingMode.Position;

            if (optionsManager != null)
            {
                optionsManager.InitializeResolutions();
                optionsManager.InitializeQuality();
                optionsManager.InitializeFPS();
            }

            if (clickSound != null) clickSound.Play();
        }
    }

    private void CloseOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.style.visibility = Visibility.Hidden;
            optionsPanel.pickingMode = PickingMode.Ignore;
            if (clickSound != null) clickSound.Play();
        }
    }

    private void OpenHowToPlayPanel()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.style.visibility = Visibility.Visible;
            howToPlayPanel.pickingMode = PickingMode.Position;

            if (optionsManager != null)
            {
                optionsManager.InitializeResolutions();
                optionsManager.InitializeQuality();
                optionsManager.InitializeFPS();
            }

            if (clickSound != null) clickSound.Play();
        }
    }

    private void CloseHowToPlayPanel()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.style.visibility = Visibility.Hidden;
            howToPlayPanel.pickingMode = PickingMode.Ignore;
            if (clickSound != null) clickSound.Play();
        }
    }

    private void OnEnable()
    {
        pauseAction?.Enable();
        playerProgress.OnAllStagesCompleted += HandleGameCompleted;


    }
    private void OnDisable()
    {
        pauseAction?.Disable();
        playerProgress.OnAllStagesCompleted -= HandleGameCompleted;
    }


    private void Update()
    {
        if (pauseAction == null) return;

        // Otimização: .triggered já detecta o clique único do frame (substitui o pauseHeld)
        if (pauseAction.triggered)
        {
            // Não pausa se o painel de cartas estiver aberto
            if (cardManager != null && cardManager.isPanelVisible) return;

            if (!isPaused)
                Pause();
            else
                HandleResume();
        }
    }

    // --- Controle de telas ---

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        pauseScreen?.Show();
        StartCoroutine(RefreshInputModule());
    }

    public void ShowGameOver()
    {
        isPaused = true;
        Time.timeScale = 0;
        gameOverScreen?.Show();
        StartCoroutine(RefreshInputModule());
    }

    private IEnumerator RefreshInputModule()
    {
        if (inputModule == null) yield break;
        inputModule.enabled = false;
        yield return null;
        inputModule.enabled = true;
    }

    private void HandleOptionsBack()
    {
        if (optionsOpenedFromPause)
            pauseScreen?.Show();
    }
    void HandleGameCompleted()
    {
        isPaused = true;
        Time.timeScale = 0;

        if (finalScreen != null)
        {
            finalScreen.style.display = DisplayStyle.Flex;
            finalScreen.pickingMode = PickingMode.Position;
        }

        StartCoroutine(RefreshInputModule());

        if (clickSound != null) clickSound.Play();
    }

    // --- Ações comuns ---

    private void HandleResume()
    {
        isPaused = false;
        Time.timeScale = 1;
        pauseScreen?.Hide();
        if (clickSound != null) clickSound.Play();
    }

    private void HandleRestart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (clickSound != null) clickSound.Play();
    }

    private void HandleMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        if (clickSound != null) clickSound.Play();
    }

    private void PlayLevel(string levelName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelName);
        if (clickSound != null) clickSound.Play();
    }
    private void QuitGame()
    {
        Application.Quit();
        

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}