using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class CameraController : MonoBehaviour
{
    [Header("Main Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Distance Settings")]
    [SerializeField] private float distanceToSplit = 8f;
    [SerializeField] private float distanceToMerge = 6f;

    private List<PlayerInput> players = new List<PlayerInput>();
    public bool isSplitScreen;

    public event Action<bool> OnSplitScreenChanged;

    private void OnEnableMainCamera()
    {
        isSplitScreen = false;
        OnSplitScreenChanged?.Invoke(isSplitScreen);
    }

    private void OnEnableSplitScreen()
    {
        isSplitScreen = true;
        OnSplitScreenChanged?.Invoke(isSplitScreen);
    }

    private void Start()
    {
        FindPlayers();
        EnableMainCamera();
    }

    private void Update()
    {
        // Caso os players ainda não tenham sido instanciados
        if (players.Count < 2)
        {
            FindPlayers();
            return;
        }

        Transform player1 = players[0].transform;
        Transform player2 = players[1].transform;

        float distance = Vector3.Distance(player1.position, player2.position);

        if (!isSplitScreen)
        {
            UpdateMainCameraPosition(player1, player2);

            if (distance > distanceToSplit)
            {
                EnableSplitScreen();
            }
        }
        else
        {
            if (distance < distanceToMerge)
            {
                EnableMainCamera();
            }
        }
    }

    private void FindPlayers()
    {
        players.Clear();

        PlayerInput[] foundPlayers = FindObjectsOfType<PlayerInput>();

        foreach (PlayerInput player in foundPlayers)
        {
            players.Add(player);
        }
    }

    private void EnableMainCamera()
    {
        isSplitScreen = false;
        OnSplitScreenChanged?.Invoke(isSplitScreen);

        if (players.Count < 2)
        {
            mainCamera.gameObject.SetActive(true);
            return;
        }

        Transform player1 = players[0].transform;
        Transform player2 = players[1].transform;

        Vector3 middlePoint = (player1.position + player2.position) / 2f;
        Vector3 desiredPosition = middlePoint + new Vector3(0f, 10f, -10f);

        mainCamera.transform.position = desiredPosition;
        mainCamera.transform.LookAt(middlePoint);

        mainCamera.gameObject.SetActive(true);

        foreach (PlayerInput player in players)
        {
            Camera playerCamera = player.GetComponentInChildren<Camera>(true);

            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
        }
    }

    private void EnableSplitScreen()
    {
        isSplitScreen = true;
        OnSplitScreenChanged?.Invoke(isSplitScreen);

        mainCamera.gameObject.SetActive(false);

        foreach (PlayerInput player in players)
        {
            Camera playerCamera = player.GetComponentInChildren<Camera>(true);

            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateMainCameraPosition(Transform player1, Transform player2)
    {
        Vector3 middlePoint = (player1.position + player2.position) / 2f;

        Vector3 desiredPosition = middlePoint + new Vector3(0f, 10f, -10f);

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPosition,
            Time.deltaTime * 5f
        );

        mainCamera.transform.LookAt(middlePoint);
    }
}