using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MenuBlurController : MonoBehaviour
{
    public Volume blurVolume;

    private DepthOfField depthOfField;

    private void Start()
    {
        blurVolume.profile.TryGet(out depthOfField);
    }

    // Ativa o desfoque
    public void AtivarBlur()
    {
        depthOfField.active = true;
        
    }

    // Desativa o desfoque
    public void DesativarBlur()
    {
        
        depthOfField.active = false;
    }
}