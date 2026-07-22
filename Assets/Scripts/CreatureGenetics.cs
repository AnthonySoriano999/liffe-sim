using UnityEngine;

public class CreatureGenetics : MonoBehaviour
{
    [Header("Not yet wired into behavior - data foundation only")]
    public float VisionRange;
    public float MovementSpeed;
    public float HungerEfficiency;
    public float ThirstEfficiency;
    public float Health;
    public float Aggression;
    public float Curiosity;
    public float SocialTendency;

    [SerializeField] private float mutationAmount = 0.08f;
    private bool inherited;

    private void Awake()
    {
        if (inherited)
        {
            return;
        }

        VisionRange = Random.Range(8f, 12f);
        MovementSpeed = Random.Range(1.2f, 1.8f);
        HungerEfficiency = Random.Range(0.85f, 1.15f);
        ThirstEfficiency = Random.Range(0.85f, 1.15f);
        Health = Random.Range(0.85f, 1.15f);
        Aggression = Random.Range(0f, 1f);
        Curiosity = Random.Range(0f, 1f);
        SocialTendency = Random.Range(0f, 1f);
    }

    public void InheritFrom(CreatureGenetics mother, CreatureGenetics father)
    {
        inherited = true;

        VisionRange = Blend(mother.VisionRange, father.VisionRange, 6f, 16f);
        MovementSpeed = Blend(mother.MovementSpeed, father.MovementSpeed, 0.8f, 2.2f);
        HungerEfficiency = Blend(mother.HungerEfficiency, father.HungerEfficiency, 0.6f, 1.4f);
        ThirstEfficiency = Blend(mother.ThirstEfficiency, father.ThirstEfficiency, 0.6f, 1.4f);
        Health = Blend(mother.Health, father.Health, 0.6f, 1.4f);
        Aggression = Blend(mother.Aggression, father.Aggression, 0f, 1f);
        Curiosity = Blend(mother.Curiosity, father.Curiosity, 0f, 1f);
        SocialTendency = Blend(mother.SocialTendency, father.SocialTendency, 0f, 1f);
    }

    private float Blend(float a, float b, float min, float max)
    {
        float mixed = Mathf.Lerp(a, b, Random.Range(0.3f, 0.7f));
        float mutated = mixed + Random.Range(-mutationAmount, mutationAmount) * (max - min);
        return Mathf.Clamp(mutated, min, max);
    }
}
