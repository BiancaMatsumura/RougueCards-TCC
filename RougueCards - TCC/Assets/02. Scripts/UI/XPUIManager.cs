using UnityEngine;
using UnityEngine.UIElements;

public class XPUIManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;

    private VisualElement fillXP;
    private Label currentXPLabel;
    private Label maxXPLabel;

    private System.Action<int, int> onXPChanged;
    private const float FillMaxWidth = 996f;



    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        fillXP = root.Q<VisualElement>("FillXP");
        currentXPLabel = root.Q<Label>("CurrentXP");
        maxXPLabel = root.Q<Label>("MaxXP");

        progress.ResetXP();

        onXPChanged = (cur, max) => UpdateXP(cur, max);

        progress.currentXP = 0;
        UpdateXP(0, progress.MaxXP);
    }

    void OnEnable()
    {
        if (progress != null) progress.OnXPChanged += onXPChanged;
    }

    void OnDisable()
    {
        if (progress != null) progress.OnXPChanged -= onXPChanged;
    }

    private void UpdateXP(int current, int max)
    {
        float percent = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        fillXP.style.width = FillMaxWidth * percent;

        if (currentXPLabel != null) currentXPLabel.text = current.ToString();
        if (maxXPLabel != null) maxXPLabel.text = $"/ {max}";
    }
}