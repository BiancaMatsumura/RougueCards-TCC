using UnityEngine;
using UnityEngine.UIElements;

public class BotaoClick : MonoBehaviour
{
    const string ClickClass = "Button--click";

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Query<Button>(className: "Button").ForEach(b =>
        {
            b.RegisterCallback<PointerDownEvent>(_ => PlayClick(b));
            b.RegisterCallback<NavigationSubmitEvent>(_ => PlayClick(b)); // teclado/gamepad
        });
    }

    void PlayClick(VisualElement ve)
    {
        ve.AddToClassList(ClickClass);
        ve.schedule.Execute(() => ve.RemoveFromClassList(ClickClass)).StartingIn(120);
    }
}