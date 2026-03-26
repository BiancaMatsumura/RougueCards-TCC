using UnityEngine;

public class CardsTrigger : MonoBehaviour
{
    private CardManager cardManager;
    [SerializeField] private PlayerProgress playerProgress;

    void Start()
    {
        cardManager = FindFirstObjectByType<CardManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cardManager.ShowPanel();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cardManager.HidePanel();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerProgress.AddXP(1);
            Debug.Log("XP adicionado! Total: " + playerProgress.currentXP);
        }
    }
}
