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

        playersContainer = root.Q<VisualElement>("PlayerContainers");
        player1UI        = root.Q<VisualElement>("Player01");
        player2UI        = root.Q<VisualElement>("Player02");

        // configura transições via código
        SetupTransitions();

        // estado inicial: painéis fora de tela
        SetOffscreen();

        if (cameraController != null)
        {
            cameraController.OnSplitScreenChanged += UpdateUI;
            UpdateUI(cameraController.isSplitScreen);
        }
    }

    private void SetupTransitions()
    {
        var duration    = new TimeValue(0.45f, TimeUnit.Second);
        var easeOut     = new EasingFunction(EasingMode.EaseOut);
        var easeInOut   = new EasingFunction(EasingMode.EaseInOut);

        // Player01: anima translate X
        player1UI.style.transitionProperty       = new List<StylePropertyName> { "translate" };
        player1UI.style.transitionDuration        = new List<TimeValue>         { duration };
        player1UI.style.transitionTimingFunction  = new List<EasingFunction>    { easeOut };

        // Player02: anima translate X
        player2UI.style.transitionProperty       = new List<StylePropertyName> { "translate" };
        player2UI.style.transitionDuration        = new List<TimeValue>         { duration };
        player2UI.style.transitionTimingFunction  = new List<EasingFunction>    { easeOut };

    
    }

    private void SetOffscreen()
    {
        // P1 começa escondido para a esquerda, P2 para a direita
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
            // cores de fundo
            playersContainer.style.backgroundColor = new Color(50f/255f, 0f, 50f/255f, 0f);
            player1UI.style.backgroundColor        = new Color(1f, 0f, 0f, 0.06f);
            player2UI.style.backgroundColor        = new Color(0f, 0f, 1f, 0.06f);

            // slide para dentro
            player1UI.style.translate = new Translate(Length.Percent(0), 0);
            player2UI.style.translate = new Translate(Length.Percent(0), 0);

        }
        else
        {
            playersContainer.style.backgroundColor = new Color(50f/255f, 0f, 50f/255f, 50f/255f);
            player1UI.style.backgroundColor        = new Color(1f, 0f, 0f, 0f);
            player2UI.style.backgroundColor        = new Color(0f, 0f, 1f, 0f);

            // slide para fora
            player1UI.style.translate = new Translate(Length.Percent(-100), 0);
            player2UI.style.translate = new Translate(Length.Percent(100), 0);

        }
    }
}