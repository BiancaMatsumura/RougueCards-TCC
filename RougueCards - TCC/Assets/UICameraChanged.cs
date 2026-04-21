using UnityEngine;
using UnityEngine.UIElements;

public class UICameraChanged : MonoBehaviour
{
    private VisualElement playersContainer;
    private VisualElement player1UI;
    private VisualElement player2UI;

    [SerializeField] private CameraController cameraController;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playersContainer = root.Q<VisualElement>("PlayerContainers");
        player1UI = root.Q<VisualElement>("Player01");
        player2UI = root.Q<VisualElement>("Player02");

        if (cameraController != null)
        {
            cameraController.OnSplitScreenChanged += UpdateUI;
            UpdateUI(cameraController.isSplitScreen);
        }
    }

    private void OnDestroy()
    {
        if (cameraController != null)
        {
            cameraController.OnSplitScreenChanged -= UpdateUI;
        }
    }

    private void UpdateUI(bool isSplitScreen)
    {
        Color containerColor = playersContainer.resolvedStyle.backgroundColor;
        Color player1Color = player1UI.resolvedStyle.backgroundColor;
        Color player2Color = player2UI.resolvedStyle.backgroundColor;

        if (isSplitScreen)
        {
            containerColor.a = 0f;
            player1Color.a = 16f / 255f;
            player2Color.a = 16f / 255f;
        }
        else
        {
            containerColor.a = 50f / 255f;
            player1Color.a = 0f;
            player2Color.a = 0f;
        }

        playersContainer.style.backgroundColor = containerColor;
        player1UI.style.backgroundColor = player1Color;
        player2UI.style.backgroundColor = player2Color;
    }
}