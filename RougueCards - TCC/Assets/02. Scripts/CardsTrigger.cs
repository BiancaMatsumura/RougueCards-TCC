using UnityEngine;

public class CardsTrigger : MonoBehaviour
{
    private CardManager cardManager;

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
}
