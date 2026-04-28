using UnityEngine;
using UnityEngine.UIElements;

public class HealthUIManager : MonoBehaviour
{
    private VisualElement fillP1;
    private VisualElement fillP2;
    private Label currentHealthLabel1;
    private Label maxHealthLabel1;
    private Label currentHealthLabel2;
    private Label maxHealthLabel2;

    private const float BAR_WIDTH = 839f;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        fillP1 = root.Q<VisualElement>("FillP1");
        fillP2 = root.Q<VisualElement>("FillP2");
        currentHealthLabel1 = root.Q<Label>("CurrentLife1");
        maxHealthLabel1 = root.Q<Label>("MaxLife1");
        currentHealthLabel2 = root.Q<Label>("CurrentLife2");
        maxHealthLabel2 = root.Q<Label>("MaxLife2");
    }

    public void RegisterPlayer(Health health)
    {
        health.OnHealthChanged += UpdateHealth;
    }

    public void UpdateHealth(int playerID, int current, int max)
    {
        float percent = (float)current / max;

        if (playerID == 1)
        {
            fillP1.style.width = percent * BAR_WIDTH;
            currentHealthLabel1.text = current.ToString();
            maxHealthLabel1.text = $"/ {max}";
        }

        if (playerID == 2)
        {
            fillP2.style.width = percent * BAR_WIDTH;
            currentHealthLabel2.text = current.ToString();
            maxHealthLabel2.text = $"/ {max}";
        }
    }
}