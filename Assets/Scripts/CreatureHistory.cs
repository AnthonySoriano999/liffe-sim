using UnityEngine;

public class CreatureHistory : MonoBehaviour
{
    public float BirthTime { get; private set; }
    public float DeathTime { get; private set; } = -1f;
    public bool HasDied { get; private set; }

    public float DistanceTraveled { get; private set; }
    public float FoodConsumed { get; private set; }
    public float WaterConsumed { get; private set; }
    public int GatherCount { get; private set; }
    public int ConflictsSurvived { get; private set; }
    public int OffspringCount { get; private set; }
    public int TimesReproduced { get; private set; }
    public int InjuriesReceived { get; private set; }

    private CreatureHealth health;
    private CreatureIdentity identity;
    private CreatureAge age;
    private CreatureGenetics genetics;
    private CreatureFamily family;

    private void Awake()
    {
        health = GetComponent<CreatureHealth>();
        identity = GetComponent<CreatureIdentity>();
        age = GetComponent<CreatureAge>();
        genetics = GetComponent<CreatureGenetics>();
        family = GetComponent<CreatureFamily>();
    }

    private void Start()
    {
        BirthTime = Time.time;
        LogBirth();
    }

    private void Update()
    {
        if (!HasDied && health != null && health.IsDead)
        {
            HasDied = true;
            DeathTime = Time.time;
            LogDeath();
        }
    }

    private void LogBirth()
    {
        if (identity == null)
        {
            return;
        }

        string parentInfo = family != null && family.MotherId >= 0
            ? "mother #" + family.MotherId.ToString("D5") + ", father #" + family.FatherId.ToString("D5")
            : "no parents (genesis)";

        Debug.Log("[Birth] " + identity.DisplayLabel() + " | " + parentInfo + " | " + GeneticsSummary());

        int lineageRootId = family != null && family.LineageRootId >= 0 ? family.LineageRootId : identity.Id;
        PopulationStats.RecordBirth(lineageRootId);
    }

    private void LogDeath()
    {
        if (identity == null)
        {
            return;
        }

        string cause = health != null ? health.DeathReason : "unknown";
        string stage = age != null ? age.CurrentStage.ToString() : "unknown";
        float finalAge = age != null ? age.Age : Lifespan;

        Debug.Log("[Death] " + identity.DisplayLabel() + " | age " + finalAge.ToString("F1") + "s (" + stage + ") | cause: " + cause
            + " | offspring: " + OffspringCount + " | " + GeneticsSummary());

        PopulationStats.RecordDeath(cause);
    }

    private string GeneticsSummary()
    {
        if (genetics == null)
        {
            return "no genetics data";
        }

        return string.Format(
            "vision={0:F1} speed={1:F2} hungerEff={2:F2} thirstEff={3:F2} health={4:F2} aggression={5:F2} curiosity={6:F2} social={7:F2}",
            genetics.VisionRange, genetics.MovementSpeed, genetics.HungerEfficiency, genetics.ThirstEfficiency,
            genetics.Health, genetics.Aggression, genetics.Curiosity, genetics.SocialTendency);
    }

    public void ReportDistance(float distance)
    {
        DistanceTraveled += distance;
    }

    public void ReportFoodConsumed(float amount)
    {
        FoodConsumed += amount;
        GatherCount++;
    }

    public void ReportWaterConsumed(float amount)
    {
        WaterConsumed += amount;
    }

    public void ReportOffspring()
    {
        OffspringCount++;
        TimesReproduced++;
    }

    public float Lifespan => (HasDied ? DeathTime : Time.time) - BirthTime;
}
