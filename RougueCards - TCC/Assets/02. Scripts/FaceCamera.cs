using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
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
}