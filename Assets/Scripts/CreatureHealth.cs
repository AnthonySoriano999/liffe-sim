using UnityEngine;

public class CreatureHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float starvationDamagePerSecond = 3f;
    [SerializeField] private float dehydrationDamagePerSecond = 3f;
    [SerializeField] private Color corpseColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    public float MaxHealth => maxHealth;
    public float Health { get; private set; }
    public bool IsDead { get; private set; }

    private CreatureStats stats;
    private SpriteRenderer spriteRenderer;

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
        if (IsDead || stats == null)
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
            Die();
        }
    }

    public void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;

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
