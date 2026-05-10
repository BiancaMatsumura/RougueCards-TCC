using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;

public class CardController
{
    public event Action<CardData> OnPickUp;

    private readonly MonoBehaviour host;
    private readonly VisualElement card;
    private readonly VisualElement front;
    private readonly VisualElement back;
    private readonly Button flipButton;
    private readonly Button pickUpButton;
    private readonly CardData data;

    private bool isFlipped = false;
    private bool isAnimating = false;

    public CardController(MonoBehaviour host, VisualElement root, CardData data, int slotIndex)
    {
        this.host = host;
        this.data = data;

        var slotName = $"Slot{slotIndex}";
        card = root.Q<VisualElement>(slotName);
        var panel = root.Q<VisualElement>($"{slotName}Panel");
        front = card.Q<VisualElement>("front");
        back = card.Q<VisualElement>("back");
        flipButton = panel.Q<Button>("flip");
        pickUpButton = panel.Q<Button>("pickUp");

        PopulateSide(front, data);
        PopulateSide(back, data);

        back.style.display = DisplayStyle.None;

        flipButton.focusable = true;
        pickUpButton.focusable = true;

        flipButton.clicked += TryFlip;
        pickUpButton.clicked += () => OnPickUp?.Invoke(data);
        card.RegisterCallback<ClickEvent>(_ => TryFlip());
    }

    // Preenche nome, descrição e imagem num lado do card
    private void PopulateSide(VisualElement side, CardData d)
    {
        var nameLabel = side.Q<Label>("CardName");
        var descLabel = side.Q<Label>("CardDescription");
        var imageEl = side.Q<VisualElement>("CardImage");

        if (nameLabel != null) nameLabel.text = d.cardName;
        if (descLabel != null) descLabel.text = d.description;
        if (imageEl != null && d.cardImage != null)
        {
            imageEl.style.backgroundImage = new StyleBackground(d.cardImage);
            imageEl.MarkDirtyRepaint(); // força o Unity a redesenhar
        }
    }

    public void FocusFlip() => flipButton.Focus();

    private void TryFlip()
    {
        if (isAnimating) return;
        host.StartCoroutine(Flip());
    }

    private IEnumerator Flip()
    {
        isAnimating = true;

        yield return Animate(1f, 0f, 0.2f);

        isFlipped = !isFlipped;
        front.style.display = isFlipped ? DisplayStyle.None : DisplayStyle.Flex;
        back.style.display = isFlipped ? DisplayStyle.Flex : DisplayStyle.None;

        yield return Animate(0f, 1f, 0.2f);

        isAnimating = false;
        flipButton.Focus();
    }

    private IEnumerator Animate(float from, float to, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float scale = Mathf.SmoothStep(from, to, time / duration);
            card.style.scale = new Scale(new Vector3(scale, 1, 1));
            yield return null;
        }
    }
}