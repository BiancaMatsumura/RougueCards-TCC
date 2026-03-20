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

    void Update()
    {

        if (player01Joined && player02Joined) return;

        foreach (var gamepad in Gamepad.all)
        {
            if (usedGamepads.Contains(gamepad))
                continue;

            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                // Player 1 entra primeiro
                if (!player01Joined)
                {
                    var player = PlayerInput.Instantiate(
                        player01Prefab,
                        controlScheme: "Player01",
                        pairWithDevice: gamepad
                    );

                    if (spawnPoints.Length > 0)
                        player.transform.position = spawnPoints[0].position;

                    player01Joined = true;

                    usedGamepads.Add(gamepad);

                    return;
                }

                // Player 2 entra depois
                if (!player02Joined)
                {
                    var player = PlayerInput.Instantiate(
                        player02Prefab,
                        controlScheme: "Player02",
                        pairWithDevice: gamepad
                    );

                    if (spawnPoints.Length > 1)
                        player.transform.position = spawnPoints[1].position;

                    player02Joined = true;

                    usedGamepads.Add(gamepad);
                    this.enabled = false;
                    return;
                }
            }
        }
    }
}