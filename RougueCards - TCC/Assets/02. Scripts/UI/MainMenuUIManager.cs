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
    public string levelToLoad = "NomeDaCena";
    void Awake()
    {
        var root = uiDocument.rootVisualElement;

        playButton = root.Q<Button>("PlayButton");
        optionButton = root.Q<Button>("OptionButton");
        creditsButton = root.Q<Button>("CreditsButton");
        quitButton = root.Q<Button>("QuitButton");

        playButton.clicked += () => PlayLevel(levelToLoad);
        optionButton.clicked += OpenOptionsMenu;
        creditsButton.clicked += OpenCreditsMenu;
        quitButton.clicked += QuitGame;
    }
    private void PlayLevel(string levelToLoad) { Time.timeScale = 1; SceneManager.LoadScene(levelToLoad); }

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
