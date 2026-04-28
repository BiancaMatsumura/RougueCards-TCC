using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UICameraChanged : MonoBehaviour
{
    private VisualElement playersContainer;
    private VisualElement player1UI;
    private VisualElement player2UI;

    [SerializeField] private SplitScreenManager cameraController;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playersContainer = root.Q<VisualElement>("HUD_PLAYERS");
        player1UI        = root.Q<VisualElement>("Player01");
        player2UI        = root.Q<VisualElement>("Player02");

        SetupTransitions();
        SetOffscreen();

        if (cameraController != null)
        {
            cameraController.OnSplitScreenChanged += UpdateUI;
            UpdateUI(cameraController.isSplitScreen);
        }
    }

    private void SetupTransitions()
    {
        var duration   = new TimeValue(0.45f, TimeUnit.Second);
        var easeOut    = new EasingFunction(EasingMode.EaseOut);

        player1UI.style.transitionProperty      = new List<StylePropertyName> { "translate" };
        player1UI.style.transitionDuration       = new List<TimeValue>         { duration };
        player1UI.style.transitionTimingFunction = new List<EasingFunction>    { easeOut };

        player2UI.style.transitionProperty      = new List<StylePropertyName> { "translate" };
        player2UI.style.transitionDuration       = new List<TimeValue>         { duration };
        player2UI.style.transitionTimingFunction = new List<EasingFunction>    { easeOut };
    }

    private void SetOffscreen()
    {
        player1UI.style.translate = new Translate(Length.Percent(-100), 0);
        player2UI.style.translate = new Translate(Length.Percent(100), 0);
    }

    private void OnDestroy()
    {
        if (cameraController != null)
            cameraController.OnSplitScreenChanged -= UpdateUI;
    }

    private void UpdateUI(bool isSplitScreen)
    {
        if (isSplitScreen)
        {
            player1UI.style.translate = new Translate(Length.Percent(0), 0);
            player2UI.style.translate = new Translate(Length.Percent(0), 0);
        }
        else
        {
            player1UI.style.translate = new Translate(Length.Percent(-100), 0);
            player2UI.style.translate = new Translate(Length.Percent(100), 0);
        }
    }
}