using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnterPlayersUI : MonoBehaviour
{
    [Header("Player 01")]
    [SerializeField] private Image instrucoes01Image;
    [SerializeField] private TextMeshProUGUI texto01;
    [SerializeField] private Sprite instrucoes01Sprite;
    [SerializeField] private Sprite personagem01Sprite;

    [Header("Player 02")]
    [SerializeField] private Image instrucoes02Image;
    [SerializeField] private TextMeshProUGUI texto02;
    [SerializeField] private Sprite instrucoes02Sprite;
    [SerializeField] private Sprite personagem02Sprite;

    [Header("Referência")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private GameObject enterPlayersPanel;

    public event System.Action OnSelectionComplete;


    void Start()
    {
        enterPlayersPanel.SetActive(true);

        instrucoes01Image.sprite = instrucoes01Sprite;
        texto01.text = "AGUARDANDO JOGADOR...";

        instrucoes02Image.sprite = instrucoes02Sprite;
        texto02.text = "ENTRE COM O PLAYER 01 PRIMEIRO";

        playerInputManager.OnPlayer01Joined += HandlePlayer01Joined;
        playerInputManager.OnPlayer02Joined += HandlePlayer02Joined;
    }

    void OnDestroy()
    {
        playerInputManager.OnPlayer01Joined -= HandlePlayer01Joined;
        playerInputManager.OnPlayer02Joined -= HandlePlayer02Joined;
    }

    private void HandlePlayer01Joined()
    {
        instrucoes01Image.sprite = personagem01Sprite;
        texto01.text = "JOGADOR SELECIONADO!";

        texto02.text = "AGUARDANDO JOGADOR...";
    }

    private void HandlePlayer02Joined()
    {
        instrucoes02Image.sprite = personagem02Sprite;
        texto02.text = "JOGADOR SELECIONADO!";

        Invoke(nameof(FinishSelection), 1.5f);
    }
    private void FinishSelection()
    {
        OnSelectionComplete?.Invoke();
        enterPlayersPanel.SetActive(false);
    }

}