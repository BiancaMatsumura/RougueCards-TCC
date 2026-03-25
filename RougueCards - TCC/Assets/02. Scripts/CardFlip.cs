using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.InputSystem.UI;

public class CardFlip : MonoBehaviour
{
    private VisualElement card;
    private VisualElement panel;
    private VisualElement front;
    private VisualElement back;

    private Button flipButton;
    private Button pickUpButton;

    private bool isFlipped = false;
    private bool isAnimating = false;

    private InputSystemUIInputModule inputModule;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        card  = root.Q<VisualElement>("Card01");
        front = root.Q<VisualElement>("front");
        back  = root.Q<VisualElement>("back");
        panel = root.Q<VisualElement>("Card01Panel");

        flipButton  = panel.Q<Button>("flip");
        pickUpButton = panel.Q<Button>("pickUp");

        back.style.display = DisplayStyle.None;

        flipButton.schedule.Execute(() => flipButton.Focus()).ExecuteLater(100);

        card.RegisterCallback<ClickEvent>(_ => TryFlip());
        flipButton.clicked += TryFlip;
    }


    private void TryFlip()
    {
        if (isAnimating) return;
        StartCoroutine(Flip());
    }

    IEnumerator Flip()
    {
        isAnimating = true;

        float duration = 0.2f;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float scale = Mathf.SmoothStep(1f, 0f, time / duration);
            card.style.scale = new Scale(new Vector3(scale, 1, 1));
            yield return null;
        }

        isFlipped = !isFlipped;
        front.style.display = isFlipped ? DisplayStyle.None : DisplayStyle.Flex;
        back.style.display  = isFlipped ? DisplayStyle.Flex  : DisplayStyle.None;

        time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float scale = Mathf.SmoothStep(0f, 1f, time / duration);
            card.style.scale = new Scale(new Vector3(scale, 1, 1));
            yield return null;
        }

        isAnimating = false;

        flipButton.Focus();
    }
}