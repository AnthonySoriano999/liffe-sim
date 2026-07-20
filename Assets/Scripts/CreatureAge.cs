using UnityEngine;

public class CreatureAge : MonoBehaviour
{
    public enum Stage { Baby, Juvenile, Adult, Old }

    [Header("Stage Durations (seconds)")]
    [SerializeField] private float babyDuration = 30f;
    [SerializeField] private float juvenileDuration = 60f;
    [SerializeField] private float adultDuration = 210f;
    [SerializeField] private float oldDuration = 120f;

    [Header("Child Appearance")]
    [SerializeField] private float childSizeMultiplier = 0.5f;
    [SerializeField] private float childSpeedMultiplier = 0.5f;
    [SerializeField] private Sprite babySprite;
    [SerializeField] private Sprite oldSprite;

    public Stage CurrentStage { get; private set; }
    public bool IsChild => CurrentStage == Stage.Baby || CurrentStage == Stage.Juvenile;
    public Transform Parent { get; set; }

    private float age;
    private Sprite adultSprite;
    private SpriteRenderer spriteRenderer;
    private CreatureWanderer wanderer;
    private CreatureHealth health;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        wanderer = GetComponent<CreatureWanderer>();
        health = GetComponent<CreatureHealth>();

        if (spriteRenderer != null)
        {
            adultSprite = spriteRenderer.sprite;
        }
    }

    private void Start()
    {
        ApplyStage(Stage.Baby);
    }

    private void Update()
    {
        if (health != null && health.IsDead)
        {
            return;
        }

        age += Time.deltaTime;

        Stage newStage = DetermineStage();
        if (newStage != CurrentStage)
        {
            ApplyStage(newStage);
        }

        float deathAge = babyDuration + juvenileDuration + adultDuration + oldDuration;
        if (age >= deathAge && health != null)
        {
            health.Die();
        }
    }

    private Stage DetermineStage()
    {
        float juvenileStart = babyDuration;
        float adultStart = juvenileStart + juvenileDuration;
        float oldStart = adultStart + adultDuration;

        if (age < juvenileStart)
        {
            return Stage.Baby;
        }
        if (age < adultStart)
        {
            return Stage.Juvenile;
        }
        if (age < oldStart)
        {
            return Stage.Adult;
        }
        return Stage.Old;
    }

    private void ApplyStage(Stage stage)
    {
        CurrentStage = stage;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = stage == Stage.Old ? oldSprite : (IsChild ? babySprite : adultSprite);
        }

        if (wanderer != null)
        {
            wanderer.SetSizeMultiplier(IsChild ? childSizeMultiplier : 1f);
            wanderer.SetSpeedMultiplier(IsChild ? childSpeedMultiplier : 1f);
        }
    }
}
