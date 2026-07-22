using UnityEngine;

public class CreatureReproduction : MonoBehaviour
{
    public enum Gender { Male, Female }

    [SerializeField] private Gender gender;
    [SerializeField] private float contentThreshold = 75f;
    [SerializeField] private float gestationTime = 15f;
    [SerializeField] private GameObject babyPrefab;

    [Header("Birth Cost")]
    [SerializeField] private float birthFoodCost = 25f;
    [SerializeField] private float birthWaterCost = 25f;

    public Gender CreatureGender => gender;
    public bool IsAvailableToMate => partner == null;
    public bool IsPregnant { get; private set; }

    private CreatureStats stats;
    private CreatureGenetics genetics;
    private CreatureAge age;
    private CreatureReproduction partner;
    private float gestationTimer;

    private void Awake()
    {
        stats = GetComponent<CreatureStats>();
        genetics = GetComponent<CreatureGenetics>();
        age = GetComponent<CreatureAge>();
    }

    private bool IsAdult()
    {
        return age == null || age.CurrentStage == CreatureAge.Stage.Adult;
    }

    private void Update()
    {
        if (!IsPregnant)
        {
            return;
        }

        gestationTimer -= Time.deltaTime;
        if (gestationTimer <= 0f)
        {
            GiveBirth();
        }
    }

    public bool IsContent()
    {
        if (stats == null)
        {
            return false;
        }

        float social = genetics != null ? genetics.SocialTendency : 0.5f;
        float effectiveThreshold = contentThreshold - (social - 0.5f) * 20f;

        return stats.Food >= effectiveThreshold && stats.Water >= effectiveThreshold;
    }

    public bool TryBondWith(CreatureReproduction other)
    {
        if (other == null || other == this || other.gender == gender)
        {
            return false;
        }

        if (PopulationCap.IsAtCapacity())
        {
            return false;
        }

        if (!IsAvailableToMate || !other.IsAvailableToMate)
        {
            return false;
        }

        if (!IsAdult() || !other.IsAdult())
        {
            return false;
        }

        if (!IsContent() || !other.IsContent())
        {
            return false;
        }

        partner = other;
        other.partner = this;

        if (gender == Gender.Female)
        {
            BeginPregnancy();
        }
        else
        {
            other.BeginPregnancy();
        }

        return true;
    }

    private void BeginPregnancy()
    {
        IsPregnant = true;
        gestationTimer = gestationTime;
    }

    private void GiveBirth()
    {
        IsPregnant = false;

        stats?.AddFood(-birthFoodCost);
        stats?.AddWater(-birthWaterCost);

        if (babyPrefab == null)
        {
            return;
        }

        GameObject baby = Instantiate(babyPrefab, transform.position, Quaternion.identity);

        CreatureAge babyAge = baby.GetComponent<CreatureAge>();
        if (babyAge != null)
        {
            babyAge.Parent = transform;
        }

        LinkFamily(baby);
        InheritGenetics(baby);

        GetComponent<CreatureHistory>()?.ReportOffspring();
        partner?.GetComponent<CreatureHistory>()?.ReportOffspring();
    }

    private void LinkFamily(GameObject baby)
    {
        CreatureIdentity motherIdentity = GetComponent<CreatureIdentity>();
        CreatureIdentity babyIdentity = baby.GetComponent<CreatureIdentity>();
        if (motherIdentity == null || babyIdentity == null)
        {
            return;
        }

        CreatureIdentity fatherIdentity = partner != null ? partner.GetComponent<CreatureIdentity>() : null;
        int fatherId = fatherIdentity != null ? fatherIdentity.Id : -1;
        string fatherName = fatherIdentity != null ? fatherIdentity.Name : "Unknown";

        CreatureFamily babyFamily = baby.GetComponent<CreatureFamily>();
        babyFamily?.SetParents(motherIdentity.Id, motherIdentity.Name, fatherId, fatherName);

        CreatureFamily motherFamily = GetComponent<CreatureFamily>();
        int lineageRootId = motherFamily != null && motherFamily.LineageRootId >= 0 ? motherFamily.LineageRootId : motherIdentity.Id;
        babyFamily?.SetLineageRoot(lineageRootId);

        motherFamily?.AddOffspring(babyIdentity.Id);
        partner?.GetComponent<CreatureFamily>()?.AddOffspring(babyIdentity.Id);
    }

    private void InheritGenetics(GameObject baby)
    {
        CreatureGenetics babyGenetics = baby.GetComponent<CreatureGenetics>();
        CreatureGenetics motherGenetics = GetComponent<CreatureGenetics>();
        CreatureGenetics fatherGenetics = partner != null ? partner.GetComponent<CreatureGenetics>() : null;

        if (babyGenetics != null && motherGenetics != null && fatherGenetics != null)
        {
            babyGenetics.InheritFrom(motherGenetics, fatherGenetics);
        }
    }
}
