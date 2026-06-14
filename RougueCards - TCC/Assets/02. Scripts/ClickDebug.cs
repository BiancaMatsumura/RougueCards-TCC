using UnityEngine;
using UnityEngine.UIElements;

public class ClickDebug : MonoBehaviour
{
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<PointerDownEvent>(e =>
        {
            Debug.Log($"[ClickDebug] Clique recebido no UI Toolkit! target: {e.target}");
        }, TrickleDown.TrickleDown);
    }
}