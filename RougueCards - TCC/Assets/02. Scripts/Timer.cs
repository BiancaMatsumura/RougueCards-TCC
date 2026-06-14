using UnityEngine;
using UnityEngine.UIElements;

public class Timer : MonoBehaviour
{
    [SerializeField] private EnterPlayersUI enterPlayersUI;

    private Label timerText;
    private bool _running = false;
    private float _startTime;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        timerText = root.Q<Label>("timerText");
    }

    void Start()
    {
        enterPlayersUI.OnSelectionComplete += StartTimer;
    }

    void OnDestroy()
    {
        enterPlayersUI.OnSelectionComplete -= StartTimer;
    }

    private void StartTimer()
    {
        _startTime = Time.timeSinceLevelLoad;
        _running = true;
    }

    void Update()
    {
        if (!_running) return;

        float time = Time.timeSinceLevelLoad - _startTime;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}