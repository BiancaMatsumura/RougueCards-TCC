using System.Collections;
using RougueCards.Attributes;
using TMPro;
using UnityEngine;

public class ShieldChargesUI : MonoBehaviour
{
    [SerializeField] private int playerID = 1;   // 1 ou 2, escolha no Inspector
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text counterText;

    private PlayerBuffs buffs;

    void Start()
    {
        root.SetActive(false);
        StartCoroutine(FindPlayerBuffs());
    }

    private IEnumerator FindPlayerBuffs()
    {
        // Espera o jogador nascer na cena
        while (buffs == null)
        {
            foreach (var stats in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
            {
                if (stats.playerID == playerID && stats.TryGetComponent(out PlayerBuffs b))
                {
                    buffs = b;
                    break;
                }
            }

            if (buffs == null) yield return new WaitForSecondsRealtime(0.25f);
        }

        buffs.OnShieldChanged += Refresh;
        Refresh(buffs.ShieldCharges);
    }

    void OnDestroy()
    {
        if (buffs != null) buffs.OnShieldChanged -= Refresh;
    }

    private void Refresh(int charges)
    {
        root.SetActive(charges > 0);
        counterText.text = $"{charges}/{buffs.ShieldMaxCharges}";
    }
}