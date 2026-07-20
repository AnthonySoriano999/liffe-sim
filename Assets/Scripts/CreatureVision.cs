using UnityEngine;

public class CreatureVision : MonoBehaviour
{
    [SerializeField] private float visionRadius = 8f;

    public WorldEntity NearestFood { get; private set; }
    public WorldEntity NearestWater { get; private set; }
    public WorldEntity NearestMate { get; private set; }

    private CreatureReproduction ownReproduction;

    private void Awake()
    {
        ownReproduction = GetComponent<CreatureReproduction>();
    }

    private void Update()
    {
        ScanForEntities();
    }

    private void ScanForEntities()
    {
        NearestFood = null;
        NearestWater = null;
        NearestMate = null;

        float nearestFoodDist = float.MaxValue;
        float nearestWaterDist = float.MaxValue;
        float nearestMateDist = float.MaxValue;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);

        foreach (Collider2D hit in hits)
        {
            WorldEntity entity = hit.GetComponent<WorldEntity>();
            if (entity == null || entity.transform == transform)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, entity.transform.position);

            switch (entity.entityType)
            {
                case WorldEntityType.Food:
                    if (distance < nearestFoodDist)
                    {
                        nearestFoodDist = distance;
                        NearestFood = entity;
                    }
                    break;

                case WorldEntityType.Water:
                    if (distance < nearestWaterDist)
                    {
                        nearestWaterDist = distance;
                        NearestWater = entity;
                    }
                    break;

                case WorldEntityType.Mate:
                    if (distance < nearestMateDist && IsOppositeGender(entity) && !IsDead(entity))
                    {
                        nearestMateDist = distance;
                        NearestMate = entity;
                    }
                    break;
            }
        }
    }

    private bool IsOppositeGender(WorldEntity entity)
    {
        if (ownReproduction == null)
        {
            return false;
        }

        CreatureReproduction otherReproduction = entity.GetComponent<CreatureReproduction>();
        return otherReproduction != null && otherReproduction.CreatureGender != ownReproduction.CreatureGender;
    }

    private bool IsDead(WorldEntity entity)
    {
        CreatureHealth otherHealth = entity.GetComponent<CreatureHealth>();
        return otherHealth != null && otherHealth.IsDead;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}
