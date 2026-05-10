using System.Collections;
using RougueCards.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Gerencia o painel de seleção de cartas e aplica os upgrades escolhidos.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class CardManager : MonoBehaviour
{
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private PlayerProgress playerProgress;
    [SerializeField] private InputActionReference toggleAction;

    private CardController[] controllers;
    private VisualElement cardPanel;
    public bool isPanelVisible = false;
    private VisualElement root;

    private System.Action<int> onStageCompletedHandler;
    private bool isLoadingCards = false;

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        cardPanel = root.Q<VisualElement>("CardsPanel");

        SetPanelVisible(false);

        // Escuta no cardPanel, não no root
        cardPanel.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                if (!isPanelVisible || isLoadingCards) return;

                if (controllers == null)
                {
                    isLoadingCards = true;
                    // adia um frame para garantir que os slots filhos também fizeram layout
                    StartCoroutine(LoadCardsNextFrame());
                }
            });
    }
    private IEnumerator LoadCardsNextFrame()
    {
        yield return null; // espera um frame

        LoadCards();
        isLoadingCards = false;

        if (controllers != null && controllers.Length > 0)
            controllers[0].FocusFlip();
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnToggleInput;
        }

        if (playerProgress != null)
        {
            onStageCompletedHandler = OnStageCompleted;
            playerProgress.OnStageCompleted += onStageCompletedHandler;
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
            toggleAction.action.performed -= OnToggleInput;

        if (playerProgress != null)
            playerProgress.OnStageCompleted -= onStageCompletedHandler;
    }

    private void OnStageCompleted(int stage)
    {
        Debug.Log($"Estágio {stage} completo! Abrindo painel de cartas...");
        ShowPanel();
    }


    // — Chamado pelo input —
    private void OnToggleInput(InputAction.CallbackContext ctx) => TogglePanel();

    // — Chamado por script externo —
    public void TogglePanel() => SetPanelVisible(!isPanelVisible);
    public void ShowPanel() => SetPanelVisible(true);
    public void HidePanel() => SetPanelVisible(false);

    // Novo método — chame sempre que quiser redefinir as cartas
    private void LoadCards()
    {
        Debug.Log("LoadCards chamado");
        var availableCards = cardDatabase.GetAvailableCards(playerProgress, maxCards: 3, randomize: true);

        controllers = new CardController[availableCards.Length];
        for (int i = 0; i < availableCards.Length; i++)
        {
            var controller = new CardController(this, root, availableCards[i], slotIndex: i);
            controller.OnPickUp += HandlePickUp;
            controllers[i] = controller;
        }
    }

    /// <summary>
    /// Liga ou desliga o painel de cartas.
    /// </summary>
    private void SetPanelVisible(bool visible)
    {
        isPanelVisible = visible;
        cardPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (visible)
        {
            controllers = null;

            if (AttributeMaestro.Instance != null)
            {
                PlayerStats decider = AttributeMaestro.Instance.GetDecidingPlayer();
                if (decider != null)
                    Debug.Log($"O jogador {decider.playerID} decide qual carta pegar!");
            }

            Time.timeScale = 0f;
        }
        else
        {
            cardPanel.Blur();
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Chamado quando uma carta é selecionada pelo jogador.
    /// </summary>
    private void HandlePickUp(CardData data)
    {
        if (AttributeMaestro.Instance != null)
        {
            AttributeMaestro.Instance.ApplySharedUpgrade(data.statToUpgrade, data.upgradeValue, data.isPercentage);
        }

        HidePanel();
        Debug.Log($"Carta coletada: {data.name}");

    }
}
