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

    // slotIndex = 0, 1, 2 — independente do nome da carta
    public CardController(MonoBehaviour host, VisualElement root, CardData data, int slotIndex)
    {
        this.host = host;
        this.data = data;

        var slotName = $"Slot{slotIndex}";
        card         = root.Q<VisualElement>(slotName);
        var panel    = root.Q<VisualElement>($"{slotName}Panel");
        front        = card.Q<VisualElement>("front");
        back         = card.Q<VisualElement>("back");
        flipButton   = panel.Q<Button>("flip");
        pickUpButton = panel.Q<Button>("pickUp");

        back.style.display = DisplayStyle.None;

        if (data.frontImage != null)
            front.style.backgroundImage = new StyleBackground(data.frontImage);
        if (data.backImage != null)
            back.style.backgroundImage = new StyleBackground(data.backImage);

        flipButton.focusable   = true;
        pickUpButton.focusable = true;

        flipButton.clicked   += TryFlip;
        pickUpButton.clicked += () => OnPickUp?.Invoke(data);
        card.RegisterCallback<ClickEvent>(_ => TryFlip());
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
        back.style.display  = isFlipped ? DisplayStyle.Flex  : DisplayStyle.None;

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