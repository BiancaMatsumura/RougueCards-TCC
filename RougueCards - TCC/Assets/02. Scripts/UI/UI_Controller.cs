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

        // Essa é a raiz do Menu Principal / Menu de Pause
        var root = uiDoc.rootVisualElement;

        // Inicializa as telas de forma segura
        var pauseVisual = root.Q<VisualElement>("PauseMenu");
        var gameOverVisual = root.Q<VisualElement>("GameOverMenu");

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

        // CORREÇÃO AQUI: O botão que ABRE as opções está no menu de pause (root)
        optionButton = root.Q<Button>("optionsButton");
        if (optionButton != null)
        {
            optionButton.clicked += OpenOptionsMenu;
        }
        else
        {
            Debug.LogWarning("Não encontrei o 'optionsButton' no UIDocument Principal. Verifique o nome no UI Builder.");
        }

        // O painel de opções em si e o botão de FECHAR estão no outro UIDocument
        if (optionsUIDocument != null)
        {
            var optionsRoot = optionsUIDocument.rootVisualElement;
            optionsPanel = optionsRoot.Q<VisualElement>("OptionsPanel");
            closeOptionsPanel = optionsRoot.Q<Button>("closeButton");

            if (closeOptionsPanel != null)
            {
                closeOptionsPanel.clicked += CloseOptionsMenu;
            }
            else
            {
                Debug.LogWarning("Não encontrei o 'closeButton' dentro do documento de Opções.");
            }
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

    private void OnEnable() => pauseAction?.Enable();
    private void OnDisable() => pauseAction?.Disable();

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
}