using UnityEngine;
using UnityEngine.UIElements;

public class BossIndicatorHUD : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Texture2D fasesFillTexture; // mesma imagem usada no background-image

    private VisualElement fasesContainer;
    private RadialFillElement fasesFill;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        fasesContainer = root.Q<VisualElement>("FasesContainer");
        fasesFill = root.Q<RadialFillElement>("Fases_Fill");
        fasesFill.fillTexture = fasesFillTexture;
        Hide();
    }

    public void Show() => fasesContainer.style.display = DisplayStyle.Flex;
    public void Hide() => fasesContainer.style.display = DisplayStyle.None;

    public void UpdateBar(float progress)
    {
        if (fasesContainer.style.display == DisplayStyle.None)
            Show();

        fasesFill.progress = progress; // 1 = cheio, 0 = vazio
    }
}