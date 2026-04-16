using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseScreen
{
    protected VisualElement root;

    public BaseScreen(VisualElement root)
    {
        this.root = root;
        RegisterCallbacks();
        Hide();
    }

    public virtual void Show()
    {
        root.style.display = DisplayStyle.Flex;
        OnShow();
    }

    public virtual void Hide()
    {
        root.style.display = DisplayStyle.None;
        OnHide();
    }

    // Subclasses sobrescrevem esses métodos
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
    protected abstract void RegisterCallbacks();
}