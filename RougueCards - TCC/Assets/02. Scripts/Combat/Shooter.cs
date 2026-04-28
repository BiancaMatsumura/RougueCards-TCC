using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    [Header("Animator")]
    public Animator control;

    [Header("Weapon Data")]
    public MeleeWeaponData weaponData;

    // sistema de combo
    private bool IsInCombo;
    private int counter;

    [Header("Combo System")]
    public int ComboLengthMax = 3;
    public float ComoboWindow = 2f;

    public bool IsShooter;
    public GameObject Hands;

    void Start()
    {
        if (weaponData != null)
        {
            ComboLengthMax += weaponData.comboMaxBonus;
            ComoboWindow *= weaponData.comboWindowMultiplier;
        }

        if (IsShooter && Hands != null)
        {
            Hands.SetActive(true);
        }
    }

    public void ATK(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!IsInCombo)
        {
            StartCoroutine(ComboTimer());
            Debug.Log("combo start");
        }

        counter++;
        ComboSteps(ComboLengthMax);
        Debug.Log(counter);
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
        if (!IsInCombo) return;

        if (counter > StepsLength)
        {
            counter = 0;
            BacktoTrigger();
            return;
        }

        string Ataque = "Atack";
        string Next = Ataque + counter;

        control.Play(Next);

        // FUTURO: dano aqui pode usar weaponData.baseDamage
    }

    void BacktoTrigger()
    {
        control.SetTrigger("Back");
    }
}