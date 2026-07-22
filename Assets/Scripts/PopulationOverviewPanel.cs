using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

public class PopulationOverviewPanel : MonoBehaviour
{
    [SerializeField] private float refreshInterval = 1f;

    private RectTransform panelRect;
    private Text bodyText;
    private float refreshTimer;

    private void Awake()
    {
        UIFactory.EnsureCanvas(gameObject, 10);
        BuildUI();
    }

    private void BuildUI()
    {
        panelRect = UIFactory.CreatePanel(transform, "PopulationPanel", new Vector2(0f, 1f), new Vector2(20f, -20f), TextAnchor.UpperLeft);
        panelRect.gameObject.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.75f);

        bodyText = UIFactory.CreateText(panelRect, "Population: 0", 13, TextAnchor.UpperLeft, new Vector2(220f, 260f));
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f)
        {
            return;
        }

        refreshTimer = refreshInterval;
        Refresh();
    }

    private void Refresh()
    {
        CreatureHealth[] allHealth = Object.FindObjectsByType<CreatureHealth>(FindObjectsSortMode.None);

        int population = 0;
        float ageSum = 0f;
        float visionSum = 0f;
        float speedSum = 0f;
        float curiositySum = 0f;
        int geneticsCount = 0;

        foreach (CreatureHealth h in allHealth)
        {
            if (h.IsDead)
            {
                continue;
            }

            population++;

            CreatureAge age = h.GetComponent<CreatureAge>();
            if (age != null)
            {
                ageSum += age.Age;
            }

            CreatureGenetics genetics = h.GetComponent<CreatureGenetics>();
            if (genetics != null)
            {
                visionSum += genetics.VisionRange;
                speedSum += genetics.MovementSpeed;
                curiositySum += genetics.Curiosity;
                geneticsCount++;
            }
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Population: " + population);
        sb.AppendLine("Avg Age: " + (population > 0 ? (ageSum / population).ToString("F0") : "0") + "s");
        sb.AppendLine("");
        sb.AppendLine("Avg Traits:");
        sb.AppendLine("Vision: " + (geneticsCount > 0 ? (visionSum / geneticsCount).ToString("F1") : "n/a"));
        sb.AppendLine("Speed: " + (geneticsCount > 0 ? (speedSum / geneticsCount).ToString("F2") : "n/a"));
        sb.AppendLine("Curiosity: " + (geneticsCount > 0 ? (curiositySum / geneticsCount).ToString("F2") : "n/a"));
        sb.AppendLine("");
        sb.AppendLine("Deaths:");
        foreach (KeyValuePair<string, int> kv in PopulationStats.GetDeathCounts())
        {
            sb.AppendLine("  " + Capitalize(kv.Key) + ": " + kv.Value);
        }

        sb.AppendLine("");
        sb.AppendLine("Top Families:");
        foreach ((string name, int count) in PopulationStats.GetTopLineages(3))
        {
            sb.AppendLine("  " + name + " lineage: " + count);
        }

        bodyText.text = sb.ToString();
    }

    private string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return char.ToUpper(text[0]) + text.Substring(1);
    }
}
