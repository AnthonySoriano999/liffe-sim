using UnityEngine;

public class CreatureReproduction : MonoBehaviour
{
    public enum Gender { Male, Female }

    [SerializeField] private Gender gender;
    [SerializeField] private float contentThreshold = 75f;
    [SerializeField] private float gestationTime = 15f;
    [SerializeField] private GameObject babyPrefab;

    public Gender CreatureGender => gender;
    public bool IsAvailableToMate => partner == null;
    public bool IsPregnant { get; private set; }

    private CreatureStats stats;
    private CreatureReproduction partner;
    private float gestationTimer;

    private void Awake()
    {
        stats = GetComponent<CreatureStats>();
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
        return stats != null && stats.Food >= contentThreshold && stats.Water >= contentThreshold;
    }

    public bool TryBondWith(CreatureReproduction other)
    {
        if (other == null || other == this || other.gender == gender)
        {
            return false;
        }

        if (!IsAvailableToMate || !other.IsAvailableToMate)
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

        if (babyPrefab != null)
        {
            GameObject baby = Instantiate(babyPrefab, transform.position, Quaternion.identity);
            CreatureAge babyAge = baby.GetComponent<CreatureAge>();
            if (babyAge != null)
            {
                babyAge.Parent = transform;
            }
        }
    }
}
