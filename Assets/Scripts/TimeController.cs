using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    [SerializeField] private float fastForwardScale = 3f;

    private void Awake()
    {
        UIFactory.EnsureCanvas(gameObject, 20);
        BuildUI();
    }

    private void BuildUI()
    {
        RectTransform panel = UIFactory.CreatePanel(transform, "TimeControls", new Vector2(0.5f, 1f), new Vector2(0f, -20f), TextAnchor.UpperCenter);
        panel.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.65f);

        RectTransform row = UIFactory.CreateHBox(panel, "Row", 8f);

        UIFactory.CreateButton(row, "Pause", new Vector2(70f, 32f), () => SetTimeScale(0f));
        UIFactory.CreateButton(row, "Play", new Vector2(70f, 32f), () => SetTimeScale(1f));
        UIFactory.CreateButton(row, "Fast Forward", new Vector2(110f, 32f), () => SetTimeScale(fastForwardScale));
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }
}
