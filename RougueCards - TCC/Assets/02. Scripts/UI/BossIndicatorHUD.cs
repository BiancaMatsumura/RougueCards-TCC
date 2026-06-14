using UnityEngine;
using UnityEngine.UI;

public class BossIndicatorHUD : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;

    public void Show() => progressSlider.gameObject.SetActive(true);
    public void Hide() => progressSlider.gameObject.SetActive(false);

    public void UpdateBar(float progress)
    {
        if (!progressSlider.gameObject.activeSelf)
            progressSlider.gameObject.SetActive(true);

        progressSlider.value = progress;
    }
}