using UnityEngine;
using UnityEngine.UIElements;

public class XPUIManager : MonoBehaviour
{
    [SerializeField] private PlayerProgress progress;


    private VisualElement fillXP;


    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        fillXP = root.Q<VisualElement>("FillXP");
        progress.currentXP = 0;
        UpdateXP(0, progress.MaxXP); 
    }

    void OnEnable()
    {
        if (progress != null) progress.OnXPChanged += (cur, max) => UpdateXP(cur, max);
    }

    void OnDisable()
    {
        if (progress != null) progress.OnXPChanged -= (cur, max) => UpdateXP(cur, max);

    }

    private void UpdateXP(int current, int max)
    {
        float percent = (float)current / max;

        fillXP.style.width = Length.Percent(percent * 100);

    }
}