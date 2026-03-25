using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;

public class CardController
{
    // Evento que o mundo externo escuta
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

    public CardController(MonoBehaviour host, VisualElement root, CardData data)
    {
        this.host = host;
        this.data = data;

        // Cada carta tem seu próprio sub-root pelo cardId
        var panel = root.Q<VisualElement>(data.cardId + "Panel");
        card = root.Q<VisualElement>(data.cardId);
        front = card.Q<VisualElement>("front");
        back = card.Q<VisualElement>("back");
        flipButton = panel.Q<Button>("flip");
        pickUpButton = panel.Q<Button>("pickUp");

        back.style.display = DisplayStyle.None;

        flipButton.clicked += TryFlip;
        pickUpButton.clicked += TryPickUp;
        card.RegisterCallback<ClickEvent>(_ => TryFlip());

        if (data.frontImage != null)
            front.style.backgroundImage = new StyleBackground(data.frontImage);

        if (data.backImage != null)
            back.style.backgroundImage = new StyleBackground(data.backImage);
    }

    public void FocusFlip() => flipButton.Focus();

    // — Flip —

    private void TryFlip()
    {
        if (isAnimating) return;
        host.StartCoroutine(Flip());
    }

    private IEnumerator Flip()
    {
        isAnimating = true;
        float duration = 0.2f;

        yield return Animate(1f, 0f, duration);

        isFlipped = !isFlipped;
        front.style.display = isFlipped ? DisplayStyle.None : DisplayStyle.Flex;
        back.style.display = isFlipped ? DisplayStyle.Flex : DisplayStyle.None;

        yield return Animate(0f, 1f, duration);

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

    // — Pick Up —

    private void TryPickUp()
    {
        OnPickUp?.Invoke(data); // dispara o evento com os dados da carta
    }
}