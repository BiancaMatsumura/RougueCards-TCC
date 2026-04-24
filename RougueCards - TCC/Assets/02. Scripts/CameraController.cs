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

    [Header("Transition")]
    [SerializeField] private float cameraSpeed = 5f;

    private List<PlayerInput> players = new List<PlayerInput>();
    public bool isSplitScreen;

    public event Action<bool> OnSplitScreenChanged;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 10f, -10f);
    [SerializeField] private Vector3 cameraStartOffset = new Vector3(0f, 20f, -20f);

    private void Start()
    {
        FindPlayers();

    }

    private void Update()
    {
        if (players.Count < 2) { FindPlayers(); return; }

        Transform player1 = players[0].transform;
        Transform player2 = players[1].transform;
        float distance = Vector3.Distance(player1.position, player2.position);

        if (!isSplitScreen)
        {
            UpdateMainCameraPosition(player1, player2);
            if (distance > distanceToSplit) EnableSplitScreen();
        }
        else
        {
            UpdatePlayerCameraPositions();
            if (distance < distanceToMerge) EnableMainCamera();
        }
    }

    private void EnableMainCamera()
    {
        isSplitScreen = false;
        OnSplitScreenChanged?.Invoke(false);

        Vector3 mid = (players[0].transform.position + players[1].transform.position) / 2f;
        mainCamera.transform.position = mid + cameraStartOffset;

        mainCamera.gameObject.SetActive(true);

        foreach (PlayerInput player in players)
        {
            Camera cam = player.GetComponentInChildren<Camera>(true);
            if (cam != null) cam.gameObject.SetActive(false);
        }
    }

    private void EnableSplitScreen()
    {
        isSplitScreen = true;
        OnSplitScreenChanged?.Invoke(true);

        mainCamera.gameObject.SetActive(false);

        foreach (PlayerInput player in players)
        {
            Camera cam = player.GetComponentInChildren<Camera>(true);
            if (cam != null)
            {
                cam.transform.position = player.transform.position + cameraStartOffset;
                cam.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateMainCameraPosition(Transform player1, Transform player2)
    {
        Vector3 mid = (player1.position + player2.position) / 2f;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            mid + cameraOffset,
            Time.deltaTime * cameraSpeed
        );

        mainCamera.transform.LookAt(mid);
    }

    private void UpdatePlayerCameraPositions()
    {
        foreach (PlayerInput player in players)
        {
            Camera cam = player.GetComponentInChildren<Camera>(true);
            if (cam == null) continue;

            cam.transform.position = Vector3.Lerp(
                cam.transform.position,
                player.transform.position + cameraOffset,
                Time.deltaTime * cameraSpeed
            );

            cam.transform.LookAt(player.transform.position);
        }
    }

    private void FindPlayers()
    {
        players.Clear();
        foreach (var p in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            players.Add(p);
    }


}