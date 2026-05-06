using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool jumpRequested;
    private HealthUIManager healthUI;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        healthUI = FindFirstObjectByType<HealthUIManager>();
        if (healthUI != null)
        {
            healthUI.UpdateHealth(1, 100, 100);
            healthUI.UpdateHealth(2, 100, 100);
        }


    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            jumpRequested = true;
        }
    }

    public void ATK(InputAction.CallbackContext context)
    {
        Debug.Log(context.phase);
        if (context.performed)
        {
            // Implement attack logic here
            Debug.Log("Attack performed!");
        }
    }

        void Update()
    {
        // Mantém o player "grudado" no chão
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Pulo
        if (jumpRequested)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false;
        }

        // Movimento
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        // Rotação suave na direção do movimento
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
        }

        // Aplica movimento horizontal
        controller.Move(move * speed * Time.deltaTime);

        // Gravidade
        velocity.y += gravity * Time.deltaTime;

        // Aplica movimento vertical
        controller.Move(velocity * Time.deltaTime);
    }

    private void Start()
    {
        var health = GetComponent<Health>();
        var ui = FindFirstObjectByType<HealthUIManager>();

        if (ui != null && health != null)
        {
            ui.RegisterPlayer(health);
        }

    }

}