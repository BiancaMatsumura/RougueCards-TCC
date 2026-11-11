using Unity.Cinemachine;
using UnityEngine;

public class CameraGroupManager : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;

    public void AddPlayer(Transform player)
    {
        targetGroup.AddMember(player, 1f, 2f);
    }

    public void RemovePlayer(Transform player)
    {
        targetGroup.RemoveMember(player);
    }
}