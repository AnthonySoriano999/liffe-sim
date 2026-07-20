using UnityEngine;
using UnityEngine.UI;

public class CreatureStatsPanel : MonoBehaviour
{
    public static CreatureStatsPanel Instance { get; private set; }

    private RectTransform panelRect;
    private Text nameLabel;
    private Text foodLabel;
    private Text waterLabel;
    private Slider foodSlider;
    private Slider waterSlider;

    private CreatureStats selectedCreature;

    private void Awake()
    {
        Instance = this;
        UIFactory.EnsureCanvas(gameObject, 10);
        BuildUI();
        panelRect.gameObject.SetActive(false);
    }

    private void BuildUI()
    {
        panelRect = UIFactory.CreatePanel(transform, "StatsPanel", new Vector2(1f, 0.5f), new Vector2(-20f, 0f), TextAnchor.UpperCenter);
        panelRect.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.65f);

        nameLabel = UIFactory.CreateText(panelRect, "Creature", 20, TextAnchor.MiddleCenter, new Vector2(200f, 26f));

        foodLabel = UIFactory.CreateText(panelRect, "Food 100/100", 15, TextAnchor.MiddleLeft, new Vector2(200f, 18f));
        foodSlider = UIFactory.CreateSlider(panelRect, new Vector2(200f, 16f), new Color(0.85f, 0.55f, 0.15f));

        waterLabel = UIFactory.CreateText(panelRect, "Water 100/100", 15, TextAnchor.MiddleLeft, new Vector2(200f, 18f));
        waterSlider = UIFactory.CreateSlider(panelRect, new Vector2(200f, 16f), new Color(0.2f, 0.55f, 0.9f));
    }

    public void ShowCreature(CreatureStats stats)
    {
        selectedCreature = stats;
        panelRect.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (selectedCreature == null)
        {
            return;
        }

        int food = Mathf.RoundToInt(selectedCreature.Food);
        int water = Mathf.RoundToInt(selectedCreature.Water);
        int max = Mathf.RoundToInt(selectedCreature.MaxStat);

        foodSlider.value = selectedCreature.Food;
        waterSlider.value = selectedCreature.Water;
        foodLabel.text = $"Food {food}/{max}";
        waterLabel.text = $"Water {water}/{max}";
    }
}
