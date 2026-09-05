using UnityEngine;
using UnityEngine.UI;

public class BossIndicatorHUD : MonoBehaviour
{
    [SerializeField] private GameObject fasesContainer;
    [SerializeField] private Image fasesFillImage;

    private void OnEnable()
    {
        Hide();
    }

    public void Show() => fasesContainer.SetActive(true);
    public void Hide() => fasesContainer.SetActive(false);

    public void UpdateBar(float progress)
    {
        if (!fasesContainer.activeSelf)
            Show();

        fasesFillImage.fillAmount = 1f - Mathf.Clamp01(progress);
    }
}