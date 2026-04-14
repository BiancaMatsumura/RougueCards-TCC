using RougueCards.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private GameObject player01Prefab;
    [SerializeField] private GameObject player02Prefab;
    [SerializeField] private Transform[] spawnPoints;

    private bool player01Joined = false;
    private bool player02Joined = false;

    private HashSet<Gamepad> usedGamepads = new HashSet<Gamepad>();
    private bool wasdUsed = false;
    private bool arrowsUsed = false;

    private enum InputType { Gamepad, KeyboardWASD, KeyboardArrows }

    void Update()
    {
        if (player01Joined && player02Joined) return;

        foreach (var gamepad in Gamepad.all)
        {
            if (usedGamepads.Contains(gamepad)) continue;
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                JoinPlayer(InputType.Gamepad, gamepad);
                return;
            }
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (!wasdUsed && keyboard.tabKey.wasPressedThisFrame)
        {
            JoinPlayer(InputType.KeyboardWASD, null);
            return;
        }

        if (!arrowsUsed && keyboard.enterKey.wasPressedThisFrame)
        {
            JoinPlayer(InputType.KeyboardArrows, null);
            return;
        }
    }

    private void JoinPlayer(InputType inputType, Gamepad gamepad)
    {
        GameObject prefab = !player01Joined ? player01Prefab : player02Prefab;
        int spawnIndex = !player01Joined ? 0 : 1;

        string scheme = inputType switch
        {
            InputType.Gamepad => !player01Joined ? "Player01" : "Player02",
            InputType.KeyboardWASD => "WASD_Player",
            InputType.KeyboardArrows => "SETAS_Player",
            _ => "WASD_Player"
        };

        // Instancia o prefab normalmente
        GameObject obj = Instantiate(prefab);

        // Pega o componente PlayerStats da INSTÂNCIA(obj), não do Prefab
        PlayerStats statsInstance = obj.GetComponent<PlayerStats>();

        if (spawnPoints.Length > spawnIndex)
            obj.transform.position = spawnPoints[spawnIndex].position;

        // Atribui ao Maestro dependendo de quem está entrando
        if (AttributeMaestro.Instance != null)
        {
            if (!player01Joined)
            {
                AttributeMaestro.Instance.player1 = statsInstance;
                statsInstance.playerID = 1;
            }
            else
            {
                AttributeMaestro.Instance.player2 = statsInstance;
                statsInstance.playerID = 2;
            }
        }

        var playerInput = obj.GetComponent<PlayerInput>();

        if (inputType == InputType.Gamepad)
        {

            playerInput.SwitchCurrentControlScheme(scheme, gamepad);
        }
        else
        {
            // Teclado: cria um InputUser separado e COMPARTILHA o dispositivo
            var user = InputUser.CreateUserWithoutPairedDevices();
            InputUser.PerformPairingWithDevice(Keyboard.current, user: user,
                options: InputUserPairingOptions.UnpairCurrentDevicesFromUser);

            user.AssociateActionsWithUser(playerInput.actions);
            user.ActivateControlScheme(scheme);
        }

        // Atualiza flags
        if (!player01Joined) player01Joined = true;
        else
        {
            player02Joined = true;
            this.enabled = false;
        }

        switch (inputType)
        {
            case InputType.Gamepad: usedGamepads.Add(gamepad); break;
            case InputType.KeyboardWASD: wasdUsed = true; break;
            case InputType.KeyboardArrows: arrowsUsed = true; break;
        }
    }
}
