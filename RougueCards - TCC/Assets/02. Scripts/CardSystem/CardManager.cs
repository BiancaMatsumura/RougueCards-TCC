using System.Collections;
using RougueCards.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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

        cardPanel.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            if (!isPanelVisible || isLoadingCards) return;

            if (controllers == null)
            {
                isLoadingCards = true;
                StartCoroutine(LoadCardsNextFrame());
            }
        });
    }

    private IEnumerator LoadCardsNextFrame()
    {
        yield return null;

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

    private void OnToggleInput(InputAction.CallbackContext ctx) => TogglePanel();

    public void TogglePanel() => SetPanelVisible(!isPanelVisible);
    public void ShowPanel() => SetPanelVisible(true);
    public void HidePanel() => SetPanelVisible(false);

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

    private void HandlePickUp(CardData data)
    {
        if (AttributeMaestro.Instance != null)
        {
            if (data.effectType == CardEffectType.StatUpgrade)
            {
                AttributeMaestro.Instance.ApplySharedUpgrade(
                    data.statToUpgrade,
                    data.upgradeValue,
                    data.isPercentage
                );
            }
            else if (data.effectType == CardEffectType.UnlockWeapon)
            {
                EquipWeapon(data);
            }
        }

        HidePanel();

        Debug.Log($"Carta coletada: {data.name}");
    }

 private void EquipWeapon(CardData data)
{
    PlayerStats player = AttributeMaestro.Instance.GetDecidingPlayer();

    if (player == null)
    {
        Debug.LogError("Player não encontrado!");
        return;
    }

    AutoShooter autoShooter = FindFirstObjectByType<AutoShooter>();

    if (autoShooter == null)
    {
        Debug.LogError("AutoShooter não encontrado!");
        return;
    }

    if (data.rangedWeapon != null)
    {
        autoShooter.SetWeapon(data.rangedWeapon);

        Debug.Log("Nova arma equipada: " + data.rangedWeapon.name);
    }
}
}