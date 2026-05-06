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

    // Variáveis para armazenar as dimensões originais do colisor
    private float baseHeight;
    private float baseRadius;
    private Vector3 baseCenter;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        healthUI = FindFirstObjectByType<HealthUIManager>();

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
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float currentSpeed = pStats != null ? pStats.stats.MoveSpeed.Value : speed;
        Vector3 move = new(moveInput.x, 0, moveInput.y);
        controller.Move(currentSpeed * Time.deltaTime * move);

        if (jumpRequested)
        {
            float h = pStats != null ? pStats.stats.JumpHeight.Value : jumpHeight;
            velocity.y = Mathf.Sqrt(h * -2f * gravity);
            jumpRequested = false;
        }

        velocity.y += gravity * Time.deltaTime;
        // Aplica movimento vertical
        controller.Move(velocity * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded) jumpRequested = true;
    }
}