using RougueCards.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

public class KillCounterUI : MonoBehaviour
{
    [SerializeField] private Label killsP1Label;
    [SerializeField] private Label killsP2Label;
    private PlayerStats _p1Stats;
    private PlayerStats _p2Stats;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        killsP1Label = root.Q<Label>("killsP1Label");
        killsP2Label = root.Q<Label>("killsP2Label");
        Debug.Log($"P1 Label: {killsP1Label}, P2 Label: {killsP2Label}");
    }

    void Start()
    {
        var splitScreen = FindFirstObjectByType<SplitScreenManager>();
        splitScreen.OnPlayersRegistered += (p1, p2) =>
        {
            _p1Stats = p1.GetComponent<PlayerStats>();
            _p2Stats = p2.GetComponent<PlayerStats>();
            // caso os players já estejam registrados
            var (p1Already, p2Already) = splitScreen.Players;
            if (p1Already != null) _p1Stats = p1Already.GetComponent<PlayerStats>();
            if (p2Already != null) _p2Stats = p2Already.GetComponent<PlayerStats>();
        };
    }

    void Update()
    {
        if (_p1Stats != null) killsP1Label.text = _p1Stats.kills.ToString("00");
        if (_p2Stats != null) killsP2Label.text = _p2Stats.kills.ToString("00");
    }
}
