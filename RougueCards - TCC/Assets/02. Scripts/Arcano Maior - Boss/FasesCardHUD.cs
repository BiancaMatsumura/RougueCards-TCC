using UnityEngine;
using UnityEngine.UIElements;

public class FasesCardHUD : MonoBehaviour
{
    [SerializeField] private UIDocument hudDocument;
    [SerializeField] private CardManager cardManager;

    [Header("Carta inicial (arraste aqui)")]
    [SerializeField] private CardData startingCard;

    [Header("Nomes dos elementos no UI Builder")]
    [SerializeField] private string imageElementName = "Fases_Card";
    [SerializeField] private string nameLabelName = "Fases_CardName";

    private VisualElement cardImageElement;
    private Label cardNameLabel;

    void OnEnable()
    {
        var root = hudDocument.rootVisualElement;
        cardImageElement = root.Q<VisualElement>(imageElementName);
        cardNameLabel = root.Q<Label>(nameLabelName);

        if (startingCard != null)
            SetCard(startingCard);
        else
            ClearCard();

        cardManager.OnBossCardPicked += SetCard;
    }

    void OnDisable()
    {
        if (cardManager != null)
            cardManager.OnBossCardPicked -= SetCard;
    }

    private void SetCard(CardData data)
    {
        if (cardImageElement != null && data.cardImage != null)
            cardImageElement.style.backgroundImage = new StyleBackground(data.cardImage);

        if (cardNameLabel != null)
            cardNameLabel.text = data.cardName;
    }

    public void ClearCard()
    {
        if (cardImageElement != null)
            cardImageElement.style.backgroundImage = StyleKeyword.Null;

        if (cardNameLabel != null)
            cardNameLabel.text = "";
    }
}