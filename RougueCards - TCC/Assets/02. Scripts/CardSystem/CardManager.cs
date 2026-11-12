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
    [SerializeField] private MenuBlurController menuBlurController;

    [SerializeField] private int staggerMs = 60;
    private VisualElement[] slotPanels;
    private Label chooserBanner;
    private Shooter shooter;

    private CardController[] controllers;
    private VisualElement cardPanel;
    public bool isPanelVisible = false;
    private VisualElement root;

    private bool isLoadingCards = false;
    public event System.Action OnPanelClosed;

    private Label p1StatusLabel;
    private Label p2StatusLabel;
    private CardData[] bossPool;

    public event System.Action<CardData> OnBossCardPicked;

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        cardPanel = root.Q<VisualElement>("CardsPanel");

        slotPanels = new[]
        {
            root.Q<VisualElement>("Slot0Panel"),
            root.Q<VisualElement>("Slot1Panel"),
            root.Q<VisualElement>("Slot2Panel")
        };
        foreach (var s in slotPanels)
        {
            s.AddToClassList("card-slot");
        }

        SetPanelVisible(false);

        cardPanel.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            if (!isPanelVisible || isLoadingCards)
            {
                return;
            }

            if (controllers == null)
            {
                isLoadingCards = true;
                StartCoroutine(LoadCardsNextFrame());
            }
        });

        p1StatusLabel = root.Q<Label>("p1StatusLabel");
        p2StatusLabel = root.Q<Label>("p2StatusLabel");
    }

    private IEnumerator LoadCardsNextFrame()
    {
        yield return null;

        LoadCards();
        PlayEnterAnimation();   // <- nova linha
        isLoadingCards = false;

        if (controllers != null && controllers.Length > 0)
        {
            controllers[0].FocusFlip();
        }
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
        {
            toggleAction.action.performed -= OnToggleInput;
        }

        if (playerProgress != null)
        {
            playerProgress.OnStageCompleted -= OnStageCompleted;
        }
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
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }

    public void ShowPanel()
    {
        menuBlurController.AtivarBlur();
        SetPanelVisible(true);

        AttributeMaestro.Instance?.LockOutNonDecidingPlayer();
        UpdateChooserBanner();
    }

    /// <summary>
    /// Atualiza o aviso no topo do painel informando qual jogador está escolhendo a carta e qual está aguardando,
    /// para ficar claro por que um dos jogadores está sem controle durante a escolha.
    /// </summary>
    private void UpdateChooserBanner()
    {
        AttributeMaestro maestro = AttributeMaestro.Instance;
        PlayerStats decider = maestro != null ? maestro.GetDecidingPlayer() : null;

        if (decider == null)
        {
            SetStatus(p1StatusLabel, null, false);
            SetStatus(p2StatusLabel, null, false);
            return;
        }

        PlayerStats waiting = maestro.LockedOutPlayer;

        // Jogo solo: só o painel de quem escolhe aparece
        // Co-op: o decisor vê "escolha", o outro vê "aguarde"
        ApplyStatusForPlayer(1, p1StatusLabel, decider, waiting);
        ApplyStatusForPlayer(2, p2StatusLabel, decider, waiting);

        Debug.Log($"decider: {(decider != null ? decider.playerID.ToString() : "null")} | waiting: {(waiting != null ? waiting.playerID.ToString() : "null")}");
    }

    private void ApplyStatusForPlayer(int playerID, Label label, PlayerStats decider, PlayerStats waiting)
    {
        if (decider.playerID == playerID)
        {
            SetStatus(label, "ESCOLHA UMA CARTA", true, "status--choosing");
        }
        else if (waiting != null && waiting.playerID == playerID)
        {
            SetStatus(label, "Aguarde o outro jogador escolher...", true, "status--waiting");
        }
        else
        {
            // Esse jogador não participa (ex.: solo)
            SetStatus(label, null, false);
        }
    }

    private void SetStatus(Label label, string text, bool visible, string stateClass = null)
    {
        if (label == null)
        {
            return;
        }

        label.RemoveFromClassList("status--choosing");
        label.RemoveFromClassList("status--waiting");

        if (!visible)
        {
            label.style.display = DisplayStyle.None;
            return;
        }

        label.text = text;
        if (stateClass != null)
        {
            label.AddToClassList(stateClass);
        }
        label.style.display = DisplayStyle.Flex;
    }

    public void HidePanel()
    {
        menuBlurController.DesativarBlur();
        SetPanelVisible(false);

        AttributeMaestro.Instance?.RestoreLockedPlayerInput();
    }

    private void LoadCards()
    {
        CardData[] availableCards;

        if (bossPool != null && bossPool.Length > 0)
            availableCards = cardDatabase.GetBossCards(bossPool, 3);
        else
            availableCards = cardDatabase.GetAvailableCards(playerProgress, 3, true);

        controllers = new CardController[availableCards.Length];

        for (int i = 0; i < availableCards.Length; i++)
        {
            var controller = new CardController(this, root, availableCards[i], i);
            controller.OnPickUp += HandlePickUp;
            controllers[i] = controller;
        }
    }

    public void SetBossPool(CardData[] pool)
    {
        bossPool = pool;
    }

    private void SetPanelVisible(bool visible)
    {
        isPanelVisible = visible;
        cardPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        Time.timeScale = visible ? 0f : 1f;

        if (!visible)
        {
            foreach (var s in slotPanels)
                s.AddToClassList("card-slot--hidden");

            controllers = null;
            bossPool = null; // <- novo
            OnPanelClosed?.Invoke();
        }
    }

    private void PlayEnterAnimation()
    {
        for (int i = 0; i < slotPanels.Length; i++)
        {
            var slot = slotPanels[i];
            slot.schedule.Execute(() => slot.RemoveFromClassList("card-slot--hidden"))
                         .StartingIn(30 + i * staggerMs);
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
                EquipWeaponMelee(data);
            }

            AttributeMaestro.Instance.CheckForCardCombos();
        }

        if (data.bossOnly)
            OnBossCardPicked?.Invoke(data);

        HidePanel();
    }

    private void EquipWeapon(CardData data)
    {
        if (autoShooter == null)
        {
            autoShooter = FindFirstObjectByType<AutoShooter>();
        }

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
    private void EquipWeaponMelee(CardData data)
    {
        if (shooter == null)
        {
            shooter = FindFirstObjectByType<Shooter>();
        }

        if (shooter != null)
        {
            shooter.ApplyWeaponAnimations();
            Debug.Log("Arma adicionada: " + data.rangedWeapon.name);
            if (data.meleeWeapon != null)
            {
                shooter.SwichWeaponMesh(data.meleeWeapon);
                shooter.SwichSlahsMesh(data.meleeWeapon);
            }
        }
        else
        {
            Debug.LogWarning("Shooter ou rangedWeapon não configurado corretamente na carta.");
        }
    }
}