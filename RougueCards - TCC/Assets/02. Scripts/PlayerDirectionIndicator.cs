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

    private readonly Color _p1Base   = new Color(0.05f, 0.07f, 0.15f);
    private readonly Color _p1Bright = new Color(0.2f, 0.3f, 1f);
    private readonly Color _p2Base   = new Color(0.16f, 0.04f, 0.12f);
    private readonly Color _p2Bright = new Color(1f, 0.2f, 0.5f);

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

        float pulse = (Mathf.Sin(Time.time * 1.5f) + 1f) / 2f;
        _arrowP1.style.unityBackgroundImageTintColor = Color.Lerp(_p1Base, _p1Bright, pulse * 0.5f);
        _arrowP2.style.unityBackgroundImageTintColor = Color.Lerp(_p2Base, _p2Bright, pulse * 0.5f);
    }

    void RotateArrow(VisualElement arrow, Transform from, Transform to)
    {
        if (arrow == null) return;

        Vector3 diff = to.position - from.position;
        float angle = Mathf.Atan2(diff.z, diff.x) * Mathf.Rad2Deg;
        arrow.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(-angle)));
    }
}