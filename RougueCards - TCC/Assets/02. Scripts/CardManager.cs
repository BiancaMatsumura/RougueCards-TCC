using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class CardManager : MonoBehaviour
{
    [SerializeField] private CardData[] cards;
    [SerializeField] private InputActionReference toggleAction; 

    private CardController[] controllers;
    private VisualElement cardPanel; 
    private bool isPanelVisible = false;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        
        cardPanel = root.Q<VisualElement>("CardsPanel");

        controllers = new CardController[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            var controller = new CardController(this, root, cards[i]);
            controller.OnPickUp += HandlePickUp;
            controllers[i] = controller;
        }

        // Começa desativado
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
    public void ShowPanel()   => SetPanelVisible(true);
    public void HidePanel()   => SetPanelVisible(false);

    private void SetPanelVisible(bool visible)
    {
        isPanelVisible = visible;
        cardPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (visible)
        {
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
        Debug.Log($"Carta coletada: {data.cardId}");
    }
}