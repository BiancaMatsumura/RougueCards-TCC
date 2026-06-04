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

    [SerializeField] private AutoShooter autoShooter;

    private CardController[] controllers;
    private VisualElement cardPanel;
    public bool isPanelVisible = false;
    private VisualElement root;

    private bool isLoadingCards = false;
    public event System.Action OnPanelClosed;

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
            playerProgress.OnStageCompleted += OnStageCompleted;
        }
    }

    void OnDisable()
    {
        if (toggleAction != null)
            toggleAction.action.performed -= OnToggleInput;

        if (playerProgress != null)
            playerProgress.OnStageCompleted -= OnStageCompleted;
    }

    private void OnStageCompleted(int stage)
    {
        Debug.Log($"Level up! Abrindo cartas...");
        ShowPanel();
    }

    private void OnToggleInput(InputAction.CallbackContext ctx)
    {
        TogglePanel();
    }

    public void TogglePanel()
    {
        if (isPanelVisible)
            HidePanel();
        else
            ShowPanel();
    }

    public void ShowPanel()
    {
        SetPanelVisible(true);
    }

    public void HidePanel()
    {
        SetPanelVisible(false);
    }

    private void LoadCards()
    {
        var availableCards =
            cardDatabase.GetAvailableCards(playerProgress, 3, true);

        controllers = new CardController[availableCards.Length];

        for (int i = 0; i < availableCards.Length; i++)
        {
            var controller = new CardController(this, root, availableCards[i], i);
            controller.OnPickUp += HandlePickUp;
            controllers[i] = controller;
        }
    }

    private void SetPanelVisible(bool visible)
    {
        isPanelVisible = visible;
        cardPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        Time.timeScale = visible ? 0f : 1f;

        if (!visible)
        {
            controllers = null;
            OnPanelClosed?.Invoke(); // linha nova
        }
    }

    private void HandlePickUp(CardData data)
    {
        Debug.Log($"Carta coletada: {data.cardName}");

        PlayerStats decider =
            AttributeMaestro.Instance.GetDecidingPlayer();

        if (decider != null)
        {
            decider.AddCardToInventory(data);

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

            AttributeMaestro.Instance.CheckForCardCombos();
        }

        HidePanel();
    }

    private void EquipWeapon(CardData data)
    {
        if (autoShooter == null)
            autoShooter = FindFirstObjectByType<AutoShooter>();

        if (autoShooter != null && data.rangedWeapon != null)
        {
            autoShooter.AddWeapon(data.rangedWeapon);
            Debug.Log("Arma adicionada: " + data.rangedWeapon.name);
        }
        else
        {
            Debug.LogWarning("AutoShooter ou rangedWeapon não configurado corretamente na carta.");
        }
    }
}