using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UI_Controller : MonoBehaviour
{
    private UIDocument uiDocument;
    [SerializeField] private InputActionAsset inputActions;

    private InputAction pauseAction;
    private InputSystemUIInputModule inputModule;
    private bool isPaused = false;
    private bool pauseHeld = false;

    private PauseScreen pauseScreen;
    private GameOverScreen gameOverScreen;
    //private OptionsScreen optionsScreen;

    // Qual tela de opções abriu as opções (para saber para onde voltar)
    private bool optionsOpenedFromPause = false;

    void Awake()
    {
        inputModule = FindFirstObjectByType<InputSystemUIInputModule>();
        var root = GetComponent<UIDocument>().rootVisualElement;

        pauseScreen    = new PauseScreen(root.Q<VisualElement>("PauseMenu"));
        gameOverScreen = new GameOverScreen(root.Q<VisualElement>("GameOverMenu"));
        //optionsScreen  = new OptionsScreen(root.Q<VisualElement>("OptionsMenu"));

        // Pause
        pauseScreen.OnResume   += HandleResume;
        pauseScreen.OnRestart  += HandleRestart;
        pauseScreen.OnMainMenu += HandleMainMenu;
        pauseScreen.OnOptions  += () => { optionsOpenedFromPause = true; OpenOptions(); };

        // Game Over
        gameOverScreen.OnRestart  += HandleRestart;
        gameOverScreen.OnMainMenu += HandleMainMenu;

        // Options
        //optionsScreen.OnBack += HandleOptionsBack;

        pauseAction = inputActions.FindAction("Pause", true);
    }

    private void OnEnable()  => pauseAction.Enable();
    private void OnDisable() => pauseAction.Disable();

    private void Update()
    {
        bool pressing = pauseAction.IsPressed();
        if (pressing && !pauseHeld)
        {
            pauseHeld = true;
            if (!isPaused) Pause();
            else HandleResume();
        }
        else if (!pressing)
        {
            pauseHeld = false;
        }
    }

    // --- Controle de telas ---

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        RefreshInputModule();
        pauseScreen.Show();
    }

    private void OpenOptions()
    {
        pauseScreen.Hide();
        //optionsScreen.Show();
    }

    private void HandleOptionsBack()
    {
        //optionsScreen.Hide();
        if (optionsOpenedFromPause)
            pauseScreen.Show();
        // Se vier do menu principal, navegue de volta lá
    }

    public void ShowGameOver()  // chame de fora quando o jogador morrer
    {
        isPaused = true;
        Time.timeScale = 0;
        RefreshInputModule();
        gameOverScreen.Show();
    }

    // --- Ações comuns ---

    private void HandleResume()
    {
        isPaused = false;
        Time.timeScale = 1;
        pauseScreen.Hide();
    }

    private void HandleRestart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    private void RefreshInputModule()
    {
        inputModule.enabled = false;
        inputModule.enabled = true;
    }

    private void PlayLevel(string levelName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelName);
    }
}