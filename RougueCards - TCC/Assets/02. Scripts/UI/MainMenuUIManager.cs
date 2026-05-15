using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private Button playButton;
    private Button optionButton;
    private Button creditsButton;
    private Button quitButton;
    private Button updateButton;
    private Button closeUpdatePanel;
    private VisualElement updatePanel;
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

        playButton.clicked += () => PlayLevel(levelToLoad);
        optionButton.clicked += OpenOptionsMenu;
        creditsButton.clicked += OpenCreditsMenu;
        quitButton.clicked += QuitGame;
        updateButton.clicked += OpenUpdatePanel;
        closeUpdatePanel.clicked += CloseUpdatePanel;

    }
    private void PlayLevel(string levelToLoad)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelToLoad);
    }

    private void OpenUpdatePanel()
    {

        updatePanel.style.display = DisplayStyle.Flex;
    }

    
    private void CloseUpdatePanel()
    {
        updatePanel.style.display = DisplayStyle.None;
    }

    private void OpenOptionsMenu()
    {
        Debug.Log("Abrir opções");
    }

    private void OpenCreditsMenu()
    {
        Debug.Log("Abrir créditos");
    }

    private void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
