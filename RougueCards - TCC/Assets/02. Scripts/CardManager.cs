using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CardManager : MonoBehaviour
{

    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private PlayerProgress playerProgress;
    [SerializeField] private InputActionReference toggleAction;

    private CardController[] controllers;
    private VisualElement cardPanel;
    private bool isPanelVisible = false;

    private VisualElement root; // guarde a referência

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        cardPanel = root.Q<VisualElement>("CardsPanel");

        SetPanelVisible(false);

        root.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            if (isPanelVisible)
                controllers[0].FocusFlip();
        });
    }

    void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnToggleInput;
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
            toggleAction.action.performed -= OnToggleInput;
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
            LoadCards(); // ← redefine as cartas toda vez que abre
            controllers[0].FocusFlip();
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
        Debug.Log($"Carta coletada: {data.name}");
    }
}