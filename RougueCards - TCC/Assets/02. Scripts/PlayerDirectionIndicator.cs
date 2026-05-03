using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDirectionIndicator : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SplitScreenManager splitManager;

    private VisualElement _arrowP1;
    private VisualElement _arrowP2;
    private Transform _p1;
    private Transform _p2;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        _arrowP1 = root.Q<VisualElement>("setaP1");
        _arrowP2 = root.Q<VisualElement>("setaP2");

        splitManager.OnSplitScreenChanged += OnSplitChanged;
    }

    void OnDisable()
    {
        splitManager.OnSplitScreenChanged -= OnSplitChanged;
    }

    void OnSplitChanged(bool isSplit)
    {
        if (isSplit)
            (_p1, _p2) = splitManager.Players;
    }

    void Update()
    {
        if (_p1 == null || _p2 == null) return;

        RotateArrow(_arrowP1, from: _p1, to: _p2);
        RotateArrow(_arrowP2, from: _p2, to: _p1);
    }

    void RotateArrow(VisualElement arrow, Transform from, Transform to)
    {
        if (arrow == null) return;

        Vector3 diff = to.position - from.position;
        float angle = Mathf.Atan2(diff.z, diff.x) * Mathf.Rad2Deg; // 2D: diff.y, diff.x
        arrow.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(-angle)));
    }
}