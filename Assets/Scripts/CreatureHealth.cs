using UnityEngine;

public class CreatureHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float starvationDamagePerSecond = 2f;
    [SerializeField] private float dehydrationDamagePerSecond = 2f;
    [SerializeField] private Color corpseColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    [SerializeField] private float corpseLifetime = 10f;

    public float MaxHealth => maxHealth;
    public float Health { get; private set; }
    public bool IsDead { get; private set; }
    public string DeathReason { get; private set; } = "";

    private CreatureStats stats;
    private SpriteRenderer spriteRenderer;
    private float corpseTimer;

    private void Awake()
    {
        stats = GetComponent<CreatureStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Health = maxHealth;
    }

    private void Update()
    {
        if (IsDead)
        {
            corpseTimer -= Time.deltaTime;
            if (corpseTimer <= 0f)
            {
                Destroy(gameObject);
            }
            return;
        }

        if (stats == null)
        {
            return;
        }

        float damage = 0f;
        if (stats.Food <= 0f)
        {
            damage += starvationDamagePerSecond;
        }
        if (stats.Water <= 0f)
        {
            damage += dehydrationDamagePerSecond;
        }

        if (damage <= 0f)
        {
            return;
        }

        Health = Mathf.Max(0f, Health - damage * Time.deltaTime);
        if (Health <= 0f)
        {
            bool starving = stats.Food <= 0f;
            bool dehydrated = stats.Water <= 0f;
            string reason = starving && dehydrated ? "starvation and dehydration" : starving ? "starvation" : "dehydration";
            Die(reason);
        }
    }

    public void Die(string reason = "unknown")
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        DeathReason = reason;
        corpseTimer = corpseLifetime;

        SetEnabled<CreatureWanderer>(false);
        SetEnabled<CreatureVision>(false);
        SetEnabled<CreatureReproduction>(false);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = corpseColor;
        }
    }

    private void SetEnabled<T>(bool value) where T : Behaviour
    {
        T component = GetComponent<T>();
        if (component != null)
        {
            component.enabled = value;
        }
    }
}
