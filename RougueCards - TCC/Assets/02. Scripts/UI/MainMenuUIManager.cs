using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument optionsUIDocument;
    [SerializeField] private OptionsManager optionsManager;
    private Button playButton;
    private Button optionButton;
    private Button creditsButton;
    private Button quitButton;
    private Button updateButton;
    private Button closeUpdatePanel;
    private VisualElement updatePanel;

    private Button closeOptionsPanel;
    private VisualElement optionsPanel;

    private Button closeCreditsPanel;
    private VisualElement creditsPanel;

    public AudioSource clickSound;

    public string levelToLoad = "NomeDaCena";
    void Awake()
    {
        var root = uiDocument.rootVisualElement;

        playButton = root.Q<Button>("PlayButton");
        optionButton = root.Q<Button>("OptionButton");
        creditsButton = root.Q<Button>("CreditsButton");
        quitButton = root.Q<Button>("QuitButton");
        updateButton = root.Q<Button>("AttButton");
        updatePanel = root.Q<VisualElement>("AttPanel");
        closeUpdatePanel = root.Q<Button>("CloseUpdatePanel");

        creditsPanel = root.Q<VisualElement>("CreditsMenu");
        closeCreditsPanel = root.Q<Button>("closeCreditsButton");

        if (optionsUIDocument != null)
        {
            var optionsRoot = optionsUIDocument.rootVisualElement;
            optionsPanel = optionsRoot.Q<VisualElement>("OptionsPanel");
            closeOptionsPanel = optionsRoot.Q<Button>("closeButton");
        }

        playButton.clicked += () => PlayLevel(levelToLoad);
        optionButton.clicked += OpenOptionsMenu;
        creditsButton.clicked += OpenCreditsMenu;
        quitButton.clicked += QuitGame;
        updateButton.clicked += OpenUpdatePanel;
        closeUpdatePanel.clicked += CloseUpdatePanel;

        if (closeOptionsPanel != null)
            closeOptionsPanel.clicked += CloseOptionsMenu;

        if (closeCreditsPanel != null)
            closeCreditsPanel.clicked += CloseCreditsMenu;  
    }
    private void PlayLevel(string levelToLoad)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelToLoad);
        clickSound.Play();
    }

    private void OpenUpdatePanel()
    {

        updatePanel.style.display = DisplayStyle.Flex;
        clickSound.Play();
    }


    private void CloseUpdatePanel()
    {
        updatePanel.style.display = DisplayStyle.None;
        clickSound.Play();
    }

    private void OpenOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.style.visibility = Visibility.Visible;
            optionsPanel.pickingMode = PickingMode.Position;
            optionsManager.InitializeResolutions();
            optionsManager.InitializeQuality();
            optionsManager.InitializeFPS();
            clickSound.Play(); 
        }
    }

    private void CloseOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.style.visibility = Visibility.Hidden;
            optionsPanel.pickingMode = PickingMode.Ignore;
            clickSound.Play();
        }
    }

    private void OpenCreditsMenu()
    {
       if (creditsPanel != null)
        {
            creditsPanel.style.visibility = Visibility.Visible;
            creditsPanel.pickingMode = PickingMode.Position;
            clickSound.Play(); 
        }
    }

    private void CloseCreditsMenu()
    {
        if (creditsPanel != null)
        {
            creditsPanel.style.visibility = Visibility.Hidden;
            creditsPanel.pickingMode = PickingMode.Ignore;
            clickSound.Play();
        }
    }

    private void QuitGame()
    {
        Application.Quit();
        clickSound.Play();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}