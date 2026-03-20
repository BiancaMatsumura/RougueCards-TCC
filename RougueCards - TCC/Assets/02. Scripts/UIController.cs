using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIController : MonoBehaviour
{
    private UIDocument uIDocument;
    private VisualElement pauseVE;
    private Button resumeBtn;
    private Button restartBtn;
    private Button mainMenuBtn;

    [SerializeField] private InputActionAsset inputActions;

    private InputAction pauseAction;
    private InputSystemUIInputModule inputModule;
    private bool isPaused = false;

    void Awake()
    {
        inputModule = FindFirstObjectByType<InputSystemUIInputModule>();
        uIDocument = GetComponent<UIDocument>();
        pauseVE = uIDocument.rootVisualElement.Q<VisualElement>("PauseMenu");

        resumeBtn = pauseVE.Q<Button>("ResumeButton");
        restartBtn = pauseVE.Q<Button>("RestartButton");
        mainMenuBtn = pauseVE.Q<Button>("MainMenu");

        pauseVE.style.display = DisplayStyle.None;

        pauseAction = inputActions.FindAction("Pause", true);
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        resumeBtn.RegisterCallback<NavigationSubmitEvent>(ResumeGame);
        restartBtn.RegisterCallback<NavigationSubmitEvent>(RestartGame);
        mainMenuBtn.RegisterCallback<NavigationSubmitEvent>(MainMenu);
    }

    private void OnDisable()
    {
        pauseAction.Disable();
        resumeBtn.UnregisterCallback<NavigationSubmitEvent>(ResumeGame);
        restartBtn.UnregisterCallback<NavigationSubmitEvent>(RestartGame);
        mainMenuBtn.UnregisterCallback<NavigationSubmitEvent>(MainMenu);
    }

    private bool pauseHeld = false;

    private void Update()
    {
        bool pressing = pauseAction.IsPressed();

        // Detecta a borda de subida manualmente (pressionou agora, não estava antes)
        if (pressing && !pauseHeld)
        {
            pauseHeld = true;

            if (!isPaused)
                Pause();
            else
                Resume();
        }
        else if (!pressing)
        {
            pauseHeld = false; 
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        pauseVE.style.display = DisplayStyle.Flex;
        inputModule.enabled = false;
        inputModule.enabled = true;
        resumeBtn.Focus();
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1;
        pauseVE.style.display = DisplayStyle.None;
    }

    private void ResumeGame(NavigationSubmitEvent evt)
    {
        Resume();
    }
    private void RestartGame(NavigationSubmitEvent evt)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void MainMenu(NavigationSubmitEvent evt)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}