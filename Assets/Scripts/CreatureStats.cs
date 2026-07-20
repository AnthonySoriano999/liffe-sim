using UnityEngine;

public class CreatureStats : MonoBehaviour
{
    [SerializeField] private float maxStat = 100f;
    [SerializeField] private float decayPerSecond = 1f;

    [Header("Hunger Behavior")]
    [SerializeField] private float minHungerThreshold = 35f;
    [SerializeField] private float maxHungerThreshold = 65f;

    [Header("Thirst Behavior")]
    [SerializeField] private float minThirstThreshold = 35f;
    [SerializeField] private float maxThirstThreshold = 65f;

    public float MaxStat => maxStat;
    public float Food { get; private set; }
    public float Water { get; private set; }
    public float HungerThreshold { get; private set; }
    public float ThirstThreshold { get; private set; }
    public bool IsHungry => Food < HungerThreshold;
    public bool IsThirsty => Water < ThirstThreshold;

    private void Start()
    {
        Food = maxStat;
        Water = maxStat;
        HungerThreshold = Random.Range(minHungerThreshold, maxHungerThreshold);
        ThirstThreshold = Random.Range(minThirstThreshold, maxThirstThreshold);
    }

    private void Update()
    {
        Food = Mathf.Clamp(Food - decayPerSecond * Time.deltaTime, 0f, maxStat);
        Water = Mathf.Clamp(Water - decayPerSecond * Time.deltaTime, 0f, maxStat);
    }

    public void AddFood(float amount)
    {
        Food = Mathf.Clamp(Food + amount, 0f, maxStat);
    }

    public void AddWater(float amount)
    {
        Water = Mathf.Clamp(Water + amount, 0f, maxStat);
    }
}
