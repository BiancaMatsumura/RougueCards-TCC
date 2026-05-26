using UnityEngine;
using UnityEngine.UIElements;

public class Timer : MonoBehaviour
{
    private Label timerText;
    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        timerText = root.Q<Label>("timerText");
    }

    void Update()
    {
        float time = Time.timeSinceLevelLoad;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
        
    }
}
