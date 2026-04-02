using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private GameObject player01Prefab;
    [SerializeField] private GameObject player02Prefab;
    [SerializeField] private Transform[] spawnPoints;

    private bool player01Joined = false;
    private bool player02Joined = false;

    private HashSet<Gamepad> usedGamepads = new HashSet<Gamepad>();
    private bool keyboardUsed = false; 

    void Update()
    {
        if (player01Joined && player02Joined) return;

        
        foreach (var gamepad in Gamepad.all)
        {
            if (usedGamepads.Contains(gamepad)) continue;

            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                JoinPlayer(gamepad);
                return;
            }
        }

        
        var keyboard = Keyboard.current;
        if (keyboard != null && !keyboardUsed)
        {
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) 
            {
                JoinPlayer(null); // null = teclado
                return;
            }
        }
    }

    private void JoinPlayer(Gamepad gamepad)
    {
        if (!player01Joined)
        {
            var player = PlayerInput.Instantiate(
                player01Prefab,
                controlScheme: "Player01",
                pairWithDevice: gamepad != null ? (InputDevice)gamepad : Keyboard.current
            );

            if (spawnPoints.Length > 0)
                player.transform.position = spawnPoints[0].position;

            player01Joined = true;

            if (gamepad != null) usedGamepads.Add(gamepad);
            else keyboardUsed = true;

            return;
        }

        if (!player02Joined)
        {
            var player = PlayerInput.Instantiate(
                player02Prefab,
                controlScheme: "Player02",
                pairWithDevice: gamepad != null ? (InputDevice)gamepad : Keyboard.current
            );

            if (spawnPoints.Length > 1)
                player.transform.position = spawnPoints[1].position;

            player02Joined = true;

            if (gamepad != null) usedGamepads.Add(gamepad);
            else keyboardUsed = true;

            this.enabled = false;
        }
    }
}