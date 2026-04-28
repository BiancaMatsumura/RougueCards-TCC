using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    public Animator control;

    // sistema de combo
    private bool IsInCombo;

    private int counter;
    [Header("Combo System")]
    public int ComboLengthMax = 3;

    public float ComoboWindow = 2f; // Janela para combo de ataque 
    public bool IsShooter;

    public GameObject Hands;

    void Start()
    {
        if (IsShooter)
        {
            Hands.SetActive(true);

        }
    }


    public void ATK(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (IsInCombo == false)
            {
                StartCoroutine(ComboTimer());
                Debug.Log("combo start");
            }

            counter++;
            ComboSteps(ComboLengthMax);
            Debug.Log(counter);
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
        if (IsInCombo == true)
        {

            if (counter > StepsLength) { counter = 0; BacktoTrigger(); return; } // Back to trigger

            string Ataque = "Atack";

            string Next = Ataque + counter;

            control.Play(Next);

        }
    }

    void BacktoTrigger()
    {
        control.SetTrigger("Back");
    }

}
