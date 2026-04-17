using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -9.8f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool jumpRequested;

    public Animator control;
    
    // sistema de combo
    private bool IsInCombo;

    private int counter;
    [Header("Combo System")]
    public int ComboLengthMax = 3;

    public float ComoboWindow = 2f; // Janela para combo de ataque 
    public bool IsShooter;

    public GameObject Hands;
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
       
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
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f; 

        if (jumpRequested)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false;
        }

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if(Input.GetKeyDown(KeyCode.Q) && IsShooter == true)
        {

            if(IsInCombo == false)
            {
                StartCoroutine(ComboTimer());
                Debug.Log("combo start");
                //control.SetBool("isAttacking", true);
            }

                
            counter++;
            ComboSteps(ComboLengthMax);
            Debug.Log(counter);
        }
    }

    private void Start()
{
    var health = GetComponent<Health>();
    var ui = FindFirstObjectByType<HealthUIManager>();

    if (ui != null && health != null)
    {
        ui.RegisterPlayer(health);
    }

    if(IsShooter)
        {   
            Hands.SetActive(true);
            
        }
}

    IEnumerator ComboTimer()
    {
        Debug.Log("combo true");
        IsInCombo = true;

        yield return new WaitForSeconds(ComoboWindow); 

        counter = 0;
        BacktoTrigger();

        IsInCombo = false;

        Debug.Log("time out Corroutine off"); 

    }

void ComboSteps(int StepsLength)
    {
        if(IsInCombo == true)
        {
           
        if(counter > StepsLength){ counter = 0; BacktoTrigger(); return; } // Back to trigger

        string Ataque = "Atack";

        string Next = Ataque + counter;

        control.Play(Next);

        Debug.Log(Next);
        }
    }

    void BacktoTrigger()
    {
      control.SetTrigger("Back");
    }
}