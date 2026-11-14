using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReviveInteraction : MonoBehaviour
{
    [SerializeField] private float reviveRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Escudo temporário")]
    [SerializeField] private float shieldDuration = 3f;  // quanto tempo o escudo dura
    [SerializeField] private int shieldMaxHits = 3;      // quantos hits ele aguenta

    public bool IsShielded { get; private set; }
    public event Action<bool> OnShieldChanged;           // opcional, para VFX

    private DownedState _myDownedState;
    private DownedState _targetBeingRevived;
    private Animator anim;
    private Coroutine _shieldCoroutine;
    private int _shieldHitsLeft;

    private void Awake()
    {
        _myDownedState = GetComponent<DownedState>();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        DeactivateShield();
    }

    public void OnReviveHeld(bool isHolding)
    {
        if (_myDownedState != null && _myDownedState.IsDowned) return;

        if (isHolding)
            TryStartRevive();
        else
            StopRevive();
    }

    private void TryStartRevive()
    {
        // Já está revivendo alguém
        if (_targetBeingRevived != null) return;

        var cols = Physics.OverlapSphere(transform.position, reviveRadius, playerLayer);

        foreach (var col in cols)
        {
            if (col.gameObject == gameObject) continue;

            var downed = col.GetComponent<DownedState>();
            if (downed == null || !downed.IsDowned) continue;

            _targetBeingRevived = downed;

            if (anim != null) anim.Play("Revive");

            // Se eu levar dano, tenta cancelar o revive (o escudo pode absorver)
            var myHealth = GetComponent<Health>();
            if (myHealth != null)
                myHealth.OnHit += InterruptOnDamage;

            ActivateShield();
            downed.BeginRevive(_myDownedState);

            // Limpa referência quando revive terminar ou expirar
            downed.OnRevived += ClearTarget;
            downed.OnDownedExpired += ClearTarget;
            return;
        }
    }

    // Soltou o botão ou foi interrompido: o progresso do alvo decai devagar, não zera
    private void StopRevive()
    {
        if (_targetBeingRevived == null) return;

        _targetBeingRevived.InterruptRevive();
        DeactivateShield(); // evita "tap de revive" só para ganhar escudo
        ClearTarget();
    }

    private void InterruptOnDamage()
    {
        if (IsShielded)
        {
            _shieldHitsLeft--;
            if (_shieldHitsLeft <= 0) DeactivateShield();
            return; // hit absorvido, revive continua
        }

        StopRevive();
    }

    private void ClearTarget()
    {
        if (anim != null) anim.SetTrigger("StopRevive");

        if (_targetBeingRevived != null)
        {
            _targetBeingRevived.OnRevived -= ClearTarget;
            _targetBeingRevived.OnDownedExpired -= ClearTarget;
        }

        var myHealth = GetComponent<Health>();
        if (myHealth != null)
            myHealth.OnHit -= InterruptOnDamage;

        _targetBeingRevived = null;
    }

    // ---------- Escudo ----------

    private void ActivateShield()
    {
        if (_shieldCoroutine != null) StopCoroutine(_shieldCoroutine);

        _shieldHitsLeft = shieldMaxHits;
        SetShield(true);
        _shieldCoroutine = StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        yield return new WaitForSeconds(shieldDuration);
        _shieldCoroutine = null;
        SetShield(false);
    }

    private void DeactivateShield()
    {
        if (_shieldCoroutine != null)
        {
            StopCoroutine(_shieldCoroutine);
            _shieldCoroutine = null;
        }
        SetShield(false);
    }

    private void SetShield(bool value)
    {
        if (IsShielded == value) return;
        IsShielded = value;
        OnShieldChanged?.Invoke(value);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, reviveRadius);
    }
}