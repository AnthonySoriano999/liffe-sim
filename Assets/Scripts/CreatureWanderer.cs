using UnityEngine;

public class CreatureWanderer : MonoBehaviour
{
    [Header("World Bounds")]
    [SerializeField] private Vector2 worldCenter = Vector2.zero;
    [SerializeField] private Vector2 worldSize = new Vector2(50f, 50f);

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float arriveThreshold = 0.1f;

    [Header("Timing")]
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 3f;

    [Header("Idle Animation")]
    [SerializeField] private float idleBreatheSpeed = 2f;
    [SerializeField] private float idleBreatheAmountY = 0.05f;
    [SerializeField] private float idleBreatheAmountX = 0.02f;

    [Header("Walk Animation")]
    [SerializeField] private float walkBounceSpeed = 8f;
    [SerializeField] private float walkBounceAmountY = 0.08f;
    [SerializeField] private float walkSquashAmountX = 0.05f;

    [Header("Mating Behavior")]
    [SerializeField] private float seekMateHungerExitThreshold = 60f;
    [SerializeField] private float seekMateTimeout = 20f;

    private enum State { Idle, Wandering, SeekingFood, SeekingWater, SeekingMate, FollowingParent }

    private State currentState;
    private Vector2 targetPosition;
    private WorldEntity targetFood;
    private WorldEntity targetWater;
    private float idleTimer;
    private float seekMateTimer;
    private Vector3 baseScale;
    private float sizeMultiplier = 1f;
    private float speedMultiplier = 1f;
    private CreatureVision vision;
    private CreatureStats stats;
    private CreatureReproduction reproduction;
    private CreatureAge age;

    private void Start()
    {
        baseScale = transform.localScale;
        vision = GetComponent<CreatureVision>();
        stats = GetComponent<CreatureStats>();
        reproduction = GetComponent<CreatureReproduction>();
        age = GetComponent<CreatureAge>();
        EnterIdle();
    }

    public void SetSizeMultiplier(float multiplier)
    {
        sizeMultiplier = multiplier;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    private void Update()
    {
        ConsiderNeeds();

        switch (currentState)
        {
            case State.Idle:
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    if (age != null && age.IsChild && age.Parent != null)
                    {
                        currentState = State.FollowingParent;
                    }
                    else
                    {
                        EnterWander();
                    }
                }
                break;

            case State.Wandering:
                MoveToward(targetPosition, EnterIdle);
                break;

            case State.SeekingFood:
                if (targetFood == null)
                {
                    EnterIdle();
                    break;
                }

                MoveToward(targetFood.transform.position, EatTargetFood);
                break;

            case State.SeekingWater:
                if (targetWater == null)
                {
                    EnterIdle();
                    break;
                }

                MoveToward(targetWater.transform.position, DrinkTargetWater);
                break;

            case State.SeekingMate:
                UpdateSeekingMate();
                break;

            case State.FollowingParent:
                if (age == null || age.Parent == null)
                {
                    EnterIdle();
                    break;
                }

                MoveToward(age.Parent.position, EnterIdle);
                break;
        }

        AnimateScale();
    }

    private void MoveToward(Vector2 destination, System.Action onArrive)
    {
        Vector2 currentPos = transform.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, destination, moveSpeed * speedMultiplier * Time.deltaTime);
        transform.position = newPos;

        if (Vector2.Distance(newPos, destination) <= arriveThreshold)
        {
            onArrive();
        }
    }

    private bool wasHungry;
    private bool wasThirsty;

    private void ConsiderNeeds()
    {
        if (currentState != State.Idle && currentState != State.Wandering && currentState != State.FollowingParent)
        {
            return;
        }

        if (stats == null || vision == null)
        {
            return;
        }

        if (stats.IsHungry && !wasHungry)
        {
            Debug.Log(gameObject.name + " is now hungry (threshold " + stats.HungerThreshold + ")");
        }
        wasHungry = stats.IsHungry;

        if (stats.IsThirsty && !wasThirsty)
        {
            Debug.Log(gameObject.name + " is now thirsty (threshold " + stats.ThirstThreshold + ")");
        }
        wasThirsty = stats.IsThirsty;

        if (stats.IsHungry && vision.NearestFood != null)
        {
            EnterSeekFood(vision.NearestFood);
            return;
        }

        if (stats.IsThirsty && vision.NearestWater != null)
        {
            Debug.Log(gameObject.name + " spotted water, going to drink");
            EnterSeekWater(vision.NearestWater);
            return;
        }

        bool isAdult = age == null || age.CurrentStage == CreatureAge.Stage.Adult;
        if (isAdult && reproduction != null && reproduction.IsAvailableToMate && reproduction.IsContent())
        {
            EnterSeekMate();
        }
    }

    private void UpdateSeekingMate()
    {
        if (reproduction == null || !reproduction.IsAvailableToMate)
        {
            EnterIdle();
            return;
        }

        if (stats != null && stats.Food < seekMateHungerExitThreshold)
        {
            EnterIdle();
            return;
        }

        seekMateTimer -= Time.deltaTime;
        if (seekMateTimer <= 0f)
        {
            EnterIdle();
            return;
        }

        WorldEntity candidate = vision != null ? vision.NearestMate : null;
        CreatureReproduction candidateReproduction = candidate != null ? candidate.GetComponent<CreatureReproduction>() : null;

        if (candidateReproduction != null && candidateReproduction.IsAvailableToMate)
        {
            MoveToward(candidate.transform.position, () => TryBondWithCandidate(candidateReproduction));
            return;
        }

        MoveToward(targetPosition, PickNewMateSearchPoint);
    }

    private void TryBondWithCandidate(CreatureReproduction candidateReproduction)
    {
        if (reproduction.TryBondWith(candidateReproduction))
        {
            Debug.Log(gameObject.name + " bonded and is starting a family!");
        }

        EnterIdle();
    }

    private void PickNewMateSearchPoint()
    {
        targetPosition = GetRandomPointInBounds();
    }

    private void AnimateScale()
    {
        bool isMoving = currentState != State.Idle;
        float waveSpeed = isMoving ? walkBounceSpeed : idleBreatheSpeed;
        float amountY = isMoving ? walkBounceAmountY : idleBreatheAmountY;
        float amountX = isMoving ? walkSquashAmountX : idleBreatheAmountX;

        float wave = Mathf.Sin(Time.time * waveSpeed);
        float offsetY = wave * amountY;
        float offsetX = -wave * amountX;

        Vector3 scaledBase = baseScale * sizeMultiplier;
        transform.localScale = new Vector3(scaledBase.x + offsetX, scaledBase.y + offsetY, scaledBase.z);
    }

    private void EnterIdle()
    {
        currentState = State.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
    }

    private void EnterWander()
    {
        currentState = State.Wandering;
        targetPosition = GetRandomPointInBounds();
    }

    private void EnterSeekFood(WorldEntity food)
    {
        currentState = State.SeekingFood;
        targetFood = food;
    }

    private void EatTargetFood()
    {
        Food food = targetFood.GetComponent<Food>();
        if (food != null && stats != null)
        {
            float nutrition = food.Consume();
            stats.AddFood(nutrition);
        }

        targetFood = null;
        EnterIdle();
    }

    private void EnterSeekWater(WorldEntity water)
    {
        currentState = State.SeekingWater;
        targetWater = water;
    }

    private void EnterSeekMate()
    {
        currentState = State.SeekingMate;
        seekMateTimer = seekMateTimeout;
        targetPosition = GetRandomPointInBounds();
    }

    private void DrinkTargetWater()
    {
        Water water = targetWater.GetComponent<Water>();
        if (water != null && stats != null)
        {
            float amount = water.Drink();
            stats.AddWater(amount);
        }

        targetWater = null;
        EnterIdle();
    }

    private Vector2 GetRandomPointInBounds()
    {
        float halfWidth = worldSize.x * 0.5f;
        float halfHeight = worldSize.y * 0.5f;
        float x = Random.Range(worldCenter.x - halfWidth, worldCenter.x + halfWidth);
        float y = Random.Range(worldCenter.y - halfHeight, worldCenter.y + halfHeight);
        return new Vector2(x, y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldCenter, worldSize);
    }
}
