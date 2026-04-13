using UnityEngine;
using UnityEngine.UIElements;

public class HealthUIManager : MonoBehaviour
{
    private VisualElement fillP1;
    private VisualElement fillP2;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        fillP1 = root.Q<VisualElement>("FillP1");
        fillP2 = root.Q<VisualElement>("FillP2");
    }

    public void RegisterPlayer(Health health)
    {
        health.OnHealthChanged += UpdateHealth;
    }

    private void UpdateHealth(int playerID, int current, int max)
    {
        float percent = (float)current / max;

        if (playerID == 1)
            fillP1.style.width = Length.Percent(percent * 100);

        if (playerID == 2)
            fillP2.style.width = Length.Percent(percent * 100);
    }
}