using UnityEngine;
using UnityEngine.UI;

public class CreatureStatsPanel : MonoBehaviour
{
    public static CreatureStatsPanel Instance { get; private set; }

    private RectTransform panelRect;
    private Text nameIdText;
    private Text ageStageText;

    private Text visionText;
    private Text speedText;
    private Text hungerEffText;
    private Text thirstEffText;
    private Text curiosityText;
    private Text socialText;

    private Text foodLabel;
    private Text waterLabel;
    private Text healthLabel;
    private Slider foodSlider;
    private Slider waterSlider;
    private Slider healthSlider;
    private Text goalText;

    private Text motherText;
    private Text fatherText;
    private Text childrenText;

    private bool hasSelection;
    private GameObject selectedGameObject;
    private CreatureIdentity selectedIdentity;
    private CreatureAge selectedAge;
    private CreatureStats selectedStats;
    private CreatureHealth selectedHealth;
    private CreatureWanderer selectedWanderer;
    private CreatureFamily selectedFamily;

    private void Awake()
    {
        Instance = this;
        UIFactory.EnsureCanvas(gameObject, 10);
        BuildUI();
        panelRect.gameObject.SetActive(false);
    }

    private void BuildUI()
    {
        panelRect = UIFactory.CreatePanel(transform, "StatsPanel", new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAnchor.UpperCenter);
        panelRect.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.75f);

        Vector2 lineSize = new Vector2(220f, 16f);

        nameIdText = UIFactory.CreateText(panelRect, "Creature", 18, TextAnchor.MiddleCenter, new Vector2(220f, 24f));
        ageStageText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleCenter, lineSize);

        UIFactory.CreateText(panelRect, "GENETICS", 12, TextAnchor.MiddleLeft, lineSize);
        visionText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
        speedText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
        hungerEffText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
        thirstEffText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
        curiosityText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
        socialText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);

        UIFactory.CreateText(panelRect, "STATE", 12, TextAnchor.MiddleLeft, lineSize);
        foodLabel = UIFactory.CreateText(panelRect, "Food", 13, TextAnchor.MiddleLeft, lineSize);
        foodSlider = UIFactory.CreateSlider(panelRect, new Vector2(220f, 14f), new Color(0.85f, 0.55f, 0.15f));
        waterLabel = UIFactory.CreateText(panelRect, "Water", 13, TextAnchor.MiddleLeft, lineSize);
        waterSlider = UIFactory.CreateSlider(panelRect, new Vector2(220f, 14f), new Color(0.2f, 0.55f, 0.9f));
        healthLabel = UIFactory.CreateText(panelRect, "Health", 13, TextAnchor.MiddleLeft, lineSize);
        healthSlider = UIFactory.CreateSlider(panelRect, new Vector2(220f, 14f), new Color(0.85f, 0.25f, 0.25f));
        goalText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);

        UIFactory.CreateText(panelRect, "FAMILY", 12, TextAnchor.MiddleLeft, lineSize);
        motherText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
        fatherText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
        childrenText = UIFactory.CreateText(panelRect, "", 13, TextAnchor.MiddleLeft, lineSize);
    }

    public void ShowCreature(GameObject creature)
    {
        if (selectedGameObject != null)
        {
            selectedGameObject.GetComponent<CreatureDebugVisuals>()?.SetVisible(false);
        }

        selectedGameObject = creature;
        creature.GetComponent<CreatureDebugVisuals>()?.SetVisible(true);

        selectedIdentity = creature.GetComponent<CreatureIdentity>();
        selectedAge = creature.GetComponent<CreatureAge>();
        CreatureGenetics genetics = creature.GetComponent<CreatureGenetics>();
        selectedStats = creature.GetComponent<CreatureStats>();
        selectedHealth = creature.GetComponent<CreatureHealth>();
        selectedWanderer = creature.GetComponent<CreatureWanderer>();
        selectedFamily = creature.GetComponent<CreatureFamily>();
        hasSelection = true;

        panelRect.gameObject.SetActive(true);

        nameIdText.text = selectedIdentity != null ? selectedIdentity.DisplayLabel() : creature.name;

        visionText.text = "Vision: " + FormatOrNA(genetics, genetics != null ? genetics.VisionRange : 0f, "F1");
        speedText.text = "Speed: " + FormatOrNA(genetics, genetics != null ? genetics.MovementSpeed : 0f, "F2");
        hungerEffText.text = "Hunger Eff: " + FormatOrNA(genetics, genetics != null ? genetics.HungerEfficiency : 0f, "F2");
        thirstEffText.text = "Thirst Eff: " + FormatOrNA(genetics, genetics != null ? genetics.ThirstEfficiency : 0f, "F2");
        curiosityText.text = "Curiosity: " + FormatOrNA(genetics, genetics != null ? genetics.Curiosity : 0f, "F2");
        socialText.text = "Social: " + FormatOrNA(genetics, genetics != null ? genetics.SocialTendency : 0f, "F2");

        bool hasParents = selectedFamily != null && selectedFamily.MotherId >= 0;
        motherText.text = "Mother: " + (hasParents ? selectedFamily.MotherName : "none (genesis)");
        fatherText.text = "Father: " + (hasParents ? selectedFamily.FatherName : "none (genesis)");
    }

    private string FormatOrNA(Object source, float value, string format)
    {
        return source != null ? value.ToString(format) : "n/a";
    }

    private void Update()
    {
        if (!hasSelection)
        {
            return;
        }

        if (selectedAge != null)
        {
            ageStageText.text = selectedAge.CurrentStage + " - " + selectedAge.Age.ToString("F0") + "s";
        }

        if (selectedStats != null)
        {
            foodSlider.value = selectedStats.Food;
            waterSlider.value = selectedStats.Water;
            foodLabel.text = "Food " + Mathf.RoundToInt(selectedStats.Food) + "%";
            waterLabel.text = "Water " + Mathf.RoundToInt(selectedStats.Water) + "%";
        }

        if (selectedHealth != null)
        {
            healthSlider.value = selectedHealth.Health;
            healthLabel.text = "Health " + Mathf.RoundToInt(selectedHealth.Health) + "%";
        }

        goalText.text = "Goal: " + (selectedWanderer != null ? selectedWanderer.GoalLabel : "unknown");

        childrenText.text = "Children: " + (selectedFamily != null ? selectedFamily.OffspringIds.Count : 0);
    }
}
