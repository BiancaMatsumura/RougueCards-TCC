using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnterPlayersUI : MonoBehaviour
{

    [Header("Fade para preto")]
    [SerializeField] private CanvasGroup fadePretoCanvasGroup;
    [SerializeField] private float duracaoFadeOut = 0.6f;
    [SerializeField] private float tempoPreto = 0.2f;     // quanto tempo fica totalmente preto
    [SerializeField] private float duracaoFadeIn = 0.6f;

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

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somPlayerEntrou;

    [Header("Animação")]
    [SerializeField] private float duracaoAnimacao = 0.4f;
    [SerializeField] private float escalaMaxima = 1.25f;
    [SerializeField] private AnimationCurve curvaAnimacao = AnimationCurve.EaseInOut(0, 0, 1, 1);

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

        fadePretoCanvasGroup.alpha = 0f;
        fadePretoCanvasGroup.blocksRaycasts = false;
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

        TocarSom();
        StartCoroutine(AnimarPop(instrucoes01Image.rectTransform));
    }

    private void HandlePlayer02Joined()
    {
        instrucoes02Image.sprite = personagem02Sprite;
        texto02.text = "JOGADOR SELECIONADO!";

        TocarSom();
        StartCoroutine(AnimarPop(instrucoes02Image.rectTransform));

        StartCoroutine(FinalizarComTransicao());
    }

    private void TocarSom()
    {
        if (audioSource != null && somPlayerEntrou != null)
            audioSource.PlayOneShot(somPlayerEntrou);
    }

    // Efeito "pop": cresce e volta ao tamanho normal
    private IEnumerator AnimarPop(RectTransform alvo)
    {
        Vector3 escalaOriginal = Vector3.one;
        Vector3 escalaAlvo = Vector3.one * escalaMaxima;
        float metade = duracaoAnimacao * 0.5f;

        // Crescendo
        for (float t = 0; t < metade; t += Time.unscaledDeltaTime)
        {
            float k = curvaAnimacao.Evaluate(t / metade);
            alvo.localScale = Vector3.LerpUnclamped(escalaOriginal, escalaAlvo, k);
            yield return null;
        }

        // Voltando
        for (float t = 0; t < metade; t += Time.unscaledDeltaTime)
        {
            float k = curvaAnimacao.Evaluate(t / metade);
            alvo.localScale = Vector3.LerpUnclamped(escalaAlvo, escalaOriginal, k);
            yield return null;
        }

        alvo.localScale = escalaOriginal;
    }

    private IEnumerator FinalizarComTransicao()
    {
        // Espera o jogador ver "JOGADOR SELECIONADO!"
        yield return new WaitForSecondsRealtime(1.5f);

        // Fade OUT: tela vai para preto
        fadePretoCanvasGroup.blocksRaycasts = true;
        yield return StartCoroutine(Fade(0f, 1f, duracaoFadeOut));

        // Com a tela toda preta, esconde o painel SEM desativar o objeto
        // (desativar aqui pararia esta corrotina e o fade in nunca aconteceria)
        CanvasGroup painelCG = enterPlayersPanel.GetComponent<CanvasGroup>();
        if (painelCG == null) painelCG = enterPlayersPanel.AddComponent<CanvasGroup>();
        painelCG.alpha = 0f;
        painelCG.interactable = false;
        painelCG.blocksRaycasts = false;

        OnSelectionComplete?.Invoke();

        yield return new WaitForSecondsRealtime(tempoPreto);

        // Fade IN: preto some, revelando o jogo
        yield return StartCoroutine(Fade(1f, 0f, duracaoFadeIn));
        fadePretoCanvasGroup.blocksRaycasts = false;

        // Só agora, com tudo terminado, desativa o painel de verdade
        enterPlayersPanel.SetActive(false);
    }

    private IEnumerator Fade(float de, float para, float duracao)
    {
        for (float t = 0; t < duracao; t += Time.unscaledDeltaTime)
        {
            fadePretoCanvasGroup.alpha = Mathf.Lerp(de, para, t / duracao);
            yield return null;
        }
        fadePretoCanvasGroup.alpha = para;
    }
}