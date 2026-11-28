using RougueCards.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ReviveIndicatorCanvas : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Slider reviveSlider;
    [SerializeField] private TMP_Text promptText;

    [Header("Texto ({0} = jogador que revive, {1} = botão)")]
    [SerializeField] private string promptFormat = "P{0}: hold [{1}]";

    private DownedState _downedState;
    private PlayerStats _downedPlayer;

    void Awake()
    {
        _downedState = GetComponentInParent<DownedState>();
        _downedPlayer = GetComponentInParent<PlayerStats>();

        _downedState.OnDowned += Show;
        _downedState.OnRevived += Hide;
        _downedState.OnDownedExpired += Hide;
        _downedState.OnReviveProgressChanged += UpdateBar;

        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (_downedState == null) return;

        _downedState.OnDowned -= Show;
        _downedState.OnRevived -= Hide;
        _downedState.OnDownedExpired -= Hide;
        _downedState.OnReviveProgressChanged -= UpdateBar;
    }

    void LateUpdate()
    {
        if (!panel.activeSelf) return;

        // pega todas as câmeras ativas e escolhe a mais próxima
        Camera nearest = null;
        float minDist = float.MaxValue;

        foreach (var cam in Camera.allCameras)
        {
            float dist = Vector3.Distance(transform.position, cam.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = cam;
            }
        }

        if (nearest != null)
            transform.forward = nearest.transform.forward;
    }

    private void Show()
    {
        RefreshPrompt();
        reviveSlider.value = 0f;
        panel.SetActive(true);
    }

    private void Hide() => panel.SetActive(false);

    private void UpdateBar(float progress) => reviveSlider.value = progress;

    private void RefreshPrompt()
    {
        if (promptText == null) return;

        promptText.text = string.Empty;

        AttributeMaestro maestro = AttributeMaestro.Instance;
        if (maestro == null || _downedPlayer == null) return;

        // Quem aperta o botão é o parceiro de quem caiu
        bool downedIsP1 = _downedPlayer.playerID == 1;
        int reviverID = downedIsP1 ? 2 : 1;

        PlayerInput input = downedIsP1 ? maestro.player2Input : maestro.player1Input;
        InputDevice device = downedIsP1 ? maestro.player2Device : maestro.player1Device;
        KeyboardLayout layout = downedIsP1 ? maestro.player2Keyboard : maestro.player1Keyboard;

        string button = GetReviveButton(input, device, layout);
        if (button == null)
        {
            Debug.LogWarning($"[Revive] Não encontrei o botão de Revive do Player {reviverID}.");
            return;
        }

        promptText.text = string.Format(promptFormat, reviverID, button);
    }

    private static string GetReviveButton(PlayerInput input, InputDevice device, KeyboardLayout layout)
    {
        if (input == null || input.actions == null) return null;

        InputAction revive = input.actions.FindAction("Revive");
        if (revive == null) return null;

        // Nomes dos grupos iguais aos do Input Actions
        string group;
        if (device is Gamepad) group = "Gamepad";
        else if (layout == KeyboardLayout.Wasd) group = "WASD_Player";
        else if (layout == KeyboardLayout.Arrows) group = "SETAS_Player";
        else return null;

        InputDevice target = device is Gamepad ? device : Keyboard.current;
        if (target == null) return null;

        foreach (InputBinding binding in revive.bindings)
        {
            if (binding.groups == null) continue;
            if (System.Array.IndexOf(binding.groups.Split(';'), group) < 0) continue;

            // Procura o botão no dispositivo real, para usar o nome daquele controle
            InputControl control = InputControlPath.TryFindControl(target, binding.effectivePath);
            if (control == null) continue;

            if (control == Keyboard.current?.rightShiftKey) return "Shift direito";

            return string.IsNullOrEmpty(control.shortDisplayName)
                ? control.displayName
                : control.shortDisplayName;
        }

        return null;
    }
}