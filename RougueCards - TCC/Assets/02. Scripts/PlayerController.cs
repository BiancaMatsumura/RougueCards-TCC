using RougueCards.Attributes;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -9.8f;


    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool jumpRequested;
    private HealthUIManager healthUI;
    private PlayerStats pStats;
    private Animator animator;

    // Variáveis para armazenar as dimensões originais do colisor
    private float baseHeight;
    private float baseRadius;
    private Vector3 baseCenter;
    private ReviveInteraction _reviveInteraction;
    private DownedState _downedState;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        healthUI = FindFirstObjectByType<HealthUIManager>();
        _reviveInteraction = GetComponent<ReviveInteraction>();
        _downedState = GetComponent<DownedState>();

        // Salva as dimensões iniciais definidas no Inspector antes de qualquer upgrade
        if (controller != null)
        {
            baseHeight = controller.height;
            baseRadius = controller.radius;
            baseCenter = controller.center;
        }

        if (healthUI != null)
        {
            healthUI.UpdateHealth(1, 100, 100);
            healthUI.UpdateHealth(2, 100, 100);
        }

    }

    private void Start()
    {
        var health = GetComponent<Health>();
        var ui = FindFirstObjectByType<HealthUIManager>();
        pStats = GetComponent<PlayerStats>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (ui != null && health != null)
        {
            ui.RegisterPlayer(health);
        }

        // Garante que o tamanho comece correto de acordo com a StatSheet
        RefreshSize();
    }

    /// <summary>
    /// Sincroniza o tamanho visual (Transform) e o tamanho físico (CharacterController)
    /// com o atributo Size do jogador.
    /// </summary>
    public void RefreshSize()
    {
        if (pStats != null && controller != null)
        {
            float newSize = pStats.stats.Size.Value;

            // 1. Atualiza a escala visual (Transform)
            transform.localScale = new Vector3(newSize, newSize, newSize);

            // 2. Atualiza as propriedades físicas do CharacterController
            // Multiplicamos os valores base pelo novo tamanho para manter a proporção
            controller.height = baseHeight * newSize;
            controller.radius = baseRadius * newSize;

            // Ajustar o centro é vital para que o colisor não "entre" no chão
            // ou fique flutuando quando o personagem cresce/diminui
            controller.center = baseCenter * newSize;

            Debug.Log($"[PlayerController] Colisor físico ajustado: Altura {controller.height}, Raio {controller.radius}");
        }
    }

    void Update()
    {
        if (_downedState != null && _downedState.IsDowned) return;
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        float currentSpeed = pStats != null ? pStats.stats.MoveSpeed.Value : speed;
        Vector3 move = new(moveInput.x, 0, moveInput.y);

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (jumpRequested && grounded)
        {
            float h = pStats != null ? pStats.stats.JumpHeight.Value : jumpHeight;
            velocity.y = Mathf.Sqrt(h * -2f * gravity);
            animator?.SetBool("IsJumping", true);
            jumpRequested = false;
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = (currentSpeed * move) + new Vector3(0, velocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);


        UpdateAnimator(move, currentSpeed); //grounded  
    }

    private void UpdateAnimator(Vector3 move, float currentSpeed) //bool grounded)
    {
        animator.SetFloat("Speed", move.magnitude * currentSpeed, 0.1f, Time.deltaTime);
        //animator.SetBool("IsGrounded", grounded);

        if (velocity.y < 0) // grounded &&
            animator.SetBool("IsJumping", false);
    }


    public void Move(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

    public void Jump(InputAction.CallbackContext context)
    {
        if (_downedState != null && _downedState.IsDowned) return;
        if (context.performed) jumpRequested = true;
    }
    public void Revive(InputAction.CallbackContext context)
    {
        if (_reviveInteraction == null) return;

        if (context.started) _reviveInteraction.OnReviveHeld(true);
        if (context.canceled) _reviveInteraction.OnReviveHeld(false);
    }

}