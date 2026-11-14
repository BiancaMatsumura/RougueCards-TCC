using System;
using System.Collections;
using UnityEngine;

public class DownedState : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float downedDuration = 20f;   // tempo até game over (só corre quando ninguém está reavivendo)
    [SerializeField] private float reviveDuration = 2f;    // tempo para reviver
    [SerializeField] private float reviveHPPercent = 0.5f;

    [Header("Decaimento do revive")]
    [SerializeField] private float decayDelay = 0.5f;      // espera antes do progresso começar a cair
    [SerializeField] private float decayRate = 0.4f;       // segundos de progresso perdidos por segundo (menor que 1 = cai mais devagar do que enche)

    public bool IsDowned { get; private set; } = false;
    public float ReviveProgress01 => reviveDuration > 0f ? Mathf.Clamp01(_reviveProgress / reviveDuration) : 0f;

    public event Action OnDowned;
    public event Action OnRevived;
    public event Action OnDownedExpired;                // timer esgotou
    public event Action<float> OnDownedTimerChanged;    // 0..1 para a UI
    public event Action<float> OnReviveProgressChanged; // 0..1 para a barra

    private Coroutine _downedCoroutine;
    private Coroutine _reviveCoroutine;
    private Coroutine _decayCoroutine;
    private DownedState _reviverState; // quem está me revivendo
    private float _reviveProgress;     // em segundos, preservado entre tentativas

    public void EnterDownedState()
    {
        Debug.Log($"[Downed] {name} entrou em estado abatido");
        if (IsDowned) return;

        IsDowned = true;
        _reviveProgress = 0f;
        OnDowned?.Invoke();
        _downedCoroutine = StartCoroutine(DownedTimerRoutine());
    }

    public void BeginRevive(DownedState reviver)
    {
        Debug.Log($"[Downed] {name} começando a ser revivido");
        if (!IsDowned || _reviveCoroutine != null) return;

        StopDecay();
        _reviverState = reviver;
        _reviveCoroutine = StartCoroutine(ReviveRoutine());
    }

    public void InterruptRevive()
    {
        Debug.Log($"[Downed] {name} revive interrompido (progresso {ReviveProgress01:P0} preservado)");
        if (_reviveCoroutine == null) return;

        StopCoroutine(_reviveCoroutine);
        _reviveCoroutine = null;
        _reviverState = null;

        // Em vez de zerar, o progresso decai devagar
        if (IsDowned && _reviveProgress > 0f)
            _decayCoroutine = StartCoroutine(DecayRoutine());
    }

    private IEnumerator DownedTimerRoutine()
    {
        float elapsed = 0f;

        while (elapsed < downedDuration)
        {
            // Timer pausa enquanto alguém está reavivendo
            if (_reviveCoroutine == null)
            {
                elapsed += Time.deltaTime;
                OnDownedTimerChanged?.Invoke(1f - elapsed / downedDuration);
            }
            yield return null;
        }

        Debug.Log($"[Downed] {name} timer esgotou sem ser revivido");
        IsDowned = false;
        _downedCoroutine = null;
        StopDecay();
        _reviveProgress = 0f;
        OnDownedExpired?.Invoke();
    }

    private IEnumerator ReviveRoutine()
    {
        while (_reviveProgress < reviveDuration)
        {
            _reviveProgress += Time.deltaTime;
            OnReviveProgressChanged?.Invoke(ReviveProgress01);
            yield return null;
        }

        Debug.Log($"[Downed] {name} revivido com sucesso!");

        if (_downedCoroutine != null)
        {
            StopCoroutine(_downedCoroutine);
            _downedCoroutine = null;
        }

        IsDowned = false;
        _reviveCoroutine = null;
        _reviverState = null;
        _reviveProgress = 0f;

        var health = GetComponent<Health>();
        if (health != null) health.Revive(reviveHPPercent);

        OnRevived?.Invoke();
        OnReviveProgressChanged?.Invoke(0f);
    }

    private IEnumerator DecayRoutine()
    {
        yield return new WaitForSeconds(decayDelay);

        while (_reviveProgress > 0f)
        {
            _reviveProgress = Mathf.Max(0f, _reviveProgress - decayRate * Time.deltaTime);
            OnReviveProgressChanged?.Invoke(ReviveProgress01);
            yield return null;
        }

        _decayCoroutine = null;
    }

    private void StopDecay()
    {
        if (_decayCoroutine == null) return;
        StopCoroutine(_decayCoroutine);
        _decayCoroutine = null;
    }
}