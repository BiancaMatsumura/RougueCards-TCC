using UnityEngine;
using UnityEngine.InputSystem;

public class ReviveInteraction : MonoBehaviour
{
    [SerializeField] private float reviveRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;


    private DownedState _myDownedState;
    private DownedState _targetBeingRevived;
    private Animator anim;

    private void Awake()
    {   
        _myDownedState = GetComponent<DownedState>();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void OnReviveHeld(bool isHolding)
    {
        if (_myDownedState != null && _myDownedState.IsDowned)
        {
            
            return;
        }

        if (isHolding)
        {
            TryStartRevive();
        }
        else
        {
            StopRevive();
        }
    }

    private void TryStartRevive()
    {
        // Já está revivendo alguém
        if (_targetBeingRevived != null)  return; 

        // Procura parceiro abatido no raio
        var cols = Physics.OverlapSphere(transform.position, reviveRadius, playerLayer);
        
        foreach (var col in cols)
        {
            if (col.gameObject == gameObject) continue;

            var downed = col.GetComponent<DownedState>();
            anim.Play("Revive");
            if (downed != null && downed.IsDowned)
            {
                
                _targetBeingRevived = downed;

                // Se eu levar dano, cancela o revive
                var myHealth = GetComponent<Health>();
                if (myHealth != null)
                    myHealth.OnHit += InterruptOnDamage;

                downed.BeginRevive(GetComponent<DownedState>());

                // Limpa referência quando revive terminar ou expirar
                downed.OnRevived += ClearTarget;
                downed.OnDownedExpired += ClearTarget;
                return;
            }
        }
        
    }

    private void StopRevive()
    {   
        if (_targetBeingRevived == null) return;
        _targetBeingRevived.InterruptRevive();
        ClearTarget();
        
    }

    private void InterruptOnDamage()
    {
        anim.SetTrigger("StopRevive");
        StopRevive();
    }

    private void ClearTarget()
    {
        anim.SetTrigger("StopRevive");
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, reviveRadius);
    }
}