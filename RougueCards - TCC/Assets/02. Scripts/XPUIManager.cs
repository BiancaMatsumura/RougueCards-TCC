using UnityEngine;
using UnityEngine.UIElements;

public class XPUIManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;

    private VisualElement fillXP;
    private Label currentXPLabel;
    private Label maxXPLabel;

    private System.Action<int, int> onXPChanged;


    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        fillXP        = root.Q<VisualElement>("FillXP");
        currentXPLabel = root.Q<Label>("CurrentXP");
        maxXPLabel     = root.Q<Label>("MaxXP");

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
        float percent = (float)current / max;

        fillXP.style.width = Length.Percent(percent * 100);

        if (currentXPLabel != null) currentXPLabel.text = current.ToString();
        if (maxXPLabel != null)     maxXPLabel.text     = max.ToString();
    }
}