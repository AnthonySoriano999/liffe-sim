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

    private enum State { Idle, Wandering, SeekingFood, SeekingWater, SeekingMate, FollowingParent, RecallingFood, RecallingWater }

    public enum TargetKind { None, Seeing, Remembering, FollowingParent }

    private State currentState;
    private State previousState;
    private Vector2 targetPosition;
    private Vector2 recallTargetPosition;
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
    private CreatureMemory memory;
    private CreatureHistory history;
    private CreatureGenetics genetics;
    private CreatureIdentity identity;
    private CreatureReproduction lastLoggedMateTarget;

    private float EffectiveMoveSpeed => genetics != null ? genetics.MovementSpeed : moveSpeed;

    public TargetKind CurrentTargetKind { get; private set; }
    public Vector2 CurrentTargetPosition { get; private set; }
    public string GoalLabel => GoalLabelFor(currentState);

    private void Start()
    {
        baseScale = transform.localScale;
        vision = GetComponent<CreatureVision>();
        stats = GetComponent<CreatureStats>();
        reproduction = GetComponent<CreatureReproduction>();
        age = GetComponent<CreatureAge>();
        memory = GetComponent<CreatureMemory>();
        history = GetComponent<CreatureHistory>();
        genetics = GetComponent<CreatureGenetics>();
        identity = GetComponent<CreatureIdentity>();
        EnterIdle();
        previousState = currentState;
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

            case State.RecallingFood:
                MoveToward(recallTargetPosition, OnArriveRecallFood);
                break;

            case State.RecallingWater:
                MoveToward(recallTargetPosition, OnArriveRecallWater);
                break;
        }

        UpdateTargetInfo();

        if (currentState != previousState)
        {
            string name = identity != null ? identity.Name : gameObject.name;
            Debug.Log(name + ": Goal changed: " + GoalLabelFor(previousState) + " -> " + GoalLabelFor(currentState));
            previousState = currentState;
        }

        AnimateScale();
    }

    private void UpdateTargetInfo()
    {
        switch (currentState)
        {
            case State.SeekingFood:
                CurrentTargetKind = targetFood != null ? TargetKind.Seeing : TargetKind.None;
                CurrentTargetPosition = targetFood != null ? (Vector2)targetFood.transform.position : Vector2.zero;
                break;

            case State.SeekingWater:
                CurrentTargetKind = targetWater != null ? TargetKind.Seeing : TargetKind.None;
                CurrentTargetPosition = targetWater != null ? (Vector2)targetWater.transform.position : Vector2.zero;
                break;

            case State.RecallingFood:
            case State.RecallingWater:
                CurrentTargetKind = TargetKind.Remembering;
                CurrentTargetPosition = recallTargetPosition;
                break;

            case State.FollowingParent:
                bool hasParent = age != null && age.Parent != null;
                CurrentTargetKind = hasParent ? TargetKind.FollowingParent : TargetKind.None;
                CurrentTargetPosition = hasParent ? (Vector2)age.Parent.position : Vector2.zero;
                break;

            case State.SeekingMate:
                WorldEntity candidate = vision != null ? vision.NearestMate : null;
                CurrentTargetKind = candidate != null ? TargetKind.Seeing : TargetKind.None;
                CurrentTargetPosition = candidate != null ? (Vector2)candidate.transform.position : Vector2.zero;
                break;

            default:
                CurrentTargetKind = TargetKind.None;
                break;
        }
    }

    private string GoalLabelFor(State state)
    {
        switch (state)
        {
            case State.Idle: return "Idle";
            case State.Wandering: return "Wandering";
            case State.SeekingFood: return "Searching food";
            case State.SeekingWater: return "Going to water";
            case State.SeekingMate: return "Seeking mate";
            case State.FollowingParent: return "Following parent";
            case State.RecallingFood: return "Returning to memory (food)";
            case State.RecallingWater: return "Returning to memory (water)";
            default: return "Unknown";
        }
    }

    private void MoveToward(Vector2 destination, System.Action onArrive)
    {
        Vector2 currentPos = transform.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, destination, EffectiveMoveSpeed * speedMultiplier * Time.deltaTime);
        transform.position = newPos;

        history?.ReportDistance(Vector2.Distance(currentPos, newPos));

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

        if (stats.IsHungry)
        {
            if (vision.NearestFood != null)
            {
                EnterSeekFood(vision.NearestFood);
                return;
            }

            if (memory != null && memory.TryRecall(MemoryType.Food, out Vector2 rememberedFood))
            {
                EnterRecallFood(rememberedFood);
                return;
            }
        }

        if (stats.IsThirsty)
        {
            if (vision.NearestWater != null)
            {
                Debug.Log(gameObject.name + " spotted water, going to drink");
                EnterSeekWater(vision.NearestWater);
                return;
            }

            if (memory != null && memory.TryRecall(MemoryType.Water, out Vector2 rememberedWater))
            {
                EnterRecallWater(rememberedWater);
                return;
            }
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
            if (candidateReproduction != lastLoggedMateTarget)
            {
                LogMateTarget(candidateReproduction, candidate.transform.position);
                lastLoggedMateTarget = candidateReproduction;
            }

            MoveToward(candidate.transform.position, () => TryBondWithCandidate(candidateReproduction));
            return;
        }

        MoveToward(targetPosition, PickNewMateSearchPoint);
    }

    private void LogMateTarget(CreatureReproduction candidateReproduction, Vector2 candidatePos)
    {
        string selfName = identity != null ? identity.Name : gameObject.name;
        CreatureIdentity candidateIdentity = candidateReproduction.GetComponent<CreatureIdentity>();
        string targetName = candidateIdentity != null ? candidateIdentity.Name : "unknown";
        float distance = Vector2.Distance(transform.position, candidatePos);

        Debug.Log(selfName + ": Searching Mate\nTarget: " + targetName + "\nDistance: " + distance.ToString("F1"));
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
        targetPosition = GetWanderTarget();
    }

    private Vector2 GetWanderTarget()
    {
        if (genetics == null)
        {
            return GetRandomPointInBounds();
        }

        float maxExtent = Mathf.Max(worldSize.x, worldSize.y);
        float explorationRadius = Mathf.Lerp(maxExtent * 0.15f, maxExtent * 0.9f, genetics.Curiosity);

        Vector2 offset = Random.insideUnitCircle * explorationRadius;
        Vector2 target = (Vector2)transform.position + offset;

        float halfWidth = worldSize.x * 0.5f;
        float halfHeight = worldSize.y * 0.5f;
        target.x = Mathf.Clamp(target.x, worldCenter.x - halfWidth, worldCenter.x + halfWidth);
        target.y = Mathf.Clamp(target.y, worldCenter.y - halfHeight, worldCenter.y + halfHeight);

        return target;
    }

    private void EnterSeekFood(WorldEntity food)
    {
        currentState = State.SeekingFood;
        targetFood = food;
    }

    private void EatTargetFood()
    {
        Vector2 eatPosition = targetFood.transform.position;
        Food food = targetFood.GetComponent<Food>();
        if (food != null && stats != null)
        {
            float nutrition = food.Consume();
            stats.AddFood(nutrition);
            history?.ReportFoodConsumed(nutrition);
        }

        memory?.Record(MemoryType.Food, eatPosition);

        targetFood = null;

        bool stillHungry = stats != null && stats.Food < stats.MaxStat;
        if (stillHungry && vision != null && vision.NearestFood != null)
        {
            EnterSeekFood(vision.NearestFood);
            return;
        }

        EnterIdle();
    }

    private void EnterSeekWater(WorldEntity water)
    {
        currentState = State.SeekingWater;
        targetWater = water;
    }

    public int MateSeekAttempts { get; private set; }

    private void EnterSeekMate()
    {
        currentState = State.SeekingMate;
        float socialMultiplier = genetics != null ? Mathf.Lerp(0.6f, 1.4f, genetics.SocialTendency) : 1f;
        seekMateTimer = seekMateTimeout * socialMultiplier;
        targetPosition = GetRandomPointInBounds();
        MateSeekAttempts++;
    }

    private void DrinkTargetWater()
    {
        Vector2 drinkPosition = targetWater.transform.position;
        Water water = targetWater.GetComponent<Water>();
        if (water != null && stats != null)
        {
            float amount = water.Drink();
            stats.AddWater(amount);
            history?.ReportWaterConsumed(amount);
        }

        memory?.Record(MemoryType.Water, drinkPosition);

        targetWater = null;
        EnterIdle();
    }

    private void EnterRecallFood(Vector2 position)
    {
        currentState = State.RecallingFood;
        recallTargetPosition = position;
    }

    private void EnterRecallWater(Vector2 position)
    {
        currentState = State.RecallingWater;
        recallTargetPosition = position;
    }

    private void OnArriveRecallFood()
    {
        if (vision != null && vision.NearestFood != null)
        {
            EnterSeekFood(vision.NearestFood);
            return;
        }

        memory?.Weaken(MemoryType.Food, recallTargetPosition);
        EnterIdle();
    }

    private void OnArriveRecallWater()
    {
        if (vision != null && vision.NearestWater != null)
        {
            EnterSeekWater(vision.NearestWater);
            return;
        }

        memory?.Weaken(MemoryType.Water, recallTargetPosition);
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
