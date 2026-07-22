using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public static class SimulationHarness
{
    private const float SimDurationSeconds = 1800f;
    private const float TimeScaleValue = 20f;
    private const int PairCount = 6;
    private const float WorldHalfSize = 24f;
    private const float RealTimeSafetyCapSeconds = 240f;
    private const float ReportIntervalSeconds = 60f;

    private static readonly List<CreatureTracker> trackers = new List<CreatureTracker>();
    private static readonly HashSet<int> knownCreatureIds = new HashSet<int>();
    private static bool started;
    private static float startRealTime;
    private static float nextReportAt;
    private static int totalMatingAttempts;
    private static int totalBonds;
    private static int totalBirths;

    private class CreatureTracker
    {
        public CreatureHealth health;
        public CreatureStats stats;
        public CreatureReproduction reproduction;
        public CreatureAge age;
        public CreatureWanderer wanderer;
        public string label;
        public bool wasAvailable = true;
    }

    public static void Run()
    {
        Debug.Log("[SimulationHarness] Run() invoked");

        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        AssetDatabase.DisallowAutoRefresh();

        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
        Debug.Log("[SimulationHarness] isPlaying set true, waiting for play mode to engage");
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        if (!started)
        {
            started = true;
            startRealTime = Time.realtimeSinceStartup;
            nextReportAt = ReportIntervalSeconds;
            Debug.Log("[SimulationHarness] Play mode confirmed, setting up scene");
            SetupScene();
            Debug.Log("[SimulationHarness] Scene setup complete: " + trackers.Count + " creatures spawned");
        }

        ScanForNewCreatures();

        if (Time.time >= nextReportAt)
        {
            ReportInterval();
            nextReportAt += ReportIntervalSeconds;
        }

        bool timeUp = Time.time >= SimDurationSeconds;
        bool safetyTimeout = Time.realtimeSinceStartup - startRealTime > RealTimeSafetyCapSeconds;

        if (timeUp || safetyTimeout)
        {
            ReportFinal(safetyTimeout);
            EditorApplication.update -= Tick;
            EditorApplication.Exit(0);
        }
    }

    private static void SetupScene()
    {
        Time.timeScale = TimeScaleValue;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        GameObject malePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/male.prefab");
        GameObject femalePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/female.prefab");
        GameObject fruitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/fruit_0.prefab");

        for (int i = 0; i < 4; i++)
        {
            GameObject bushGo = new GameObject("Bush_" + i, typeof(BushSpawner));
            bushGo.transform.position = RandomPoint();

            SerializedObject so = new SerializedObject(bushGo.GetComponent<BushSpawner>());
            so.FindProperty("foodPrefab").objectReferenceValue = fruitPrefab;
            so.ApplyModifiedProperties();
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject waterGo = new GameObject("Water_" + i, typeof(CircleCollider2D), typeof(WorldEntity), typeof(Water));
            waterGo.transform.position = RandomPoint();

            CircleCollider2D col = waterGo.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.5f;
            waterGo.GetComponent<WorldEntity>().entityType = WorldEntityType.Water;
        }

        for (int i = 0; i < PairCount; i++)
        {
            GameObject male = Object.Instantiate(malePrefab, RandomPoint(), Quaternion.identity);
            male.name = "Male_" + i;
            RegisterCreature(male, "Male_" + i);

            GameObject female = Object.Instantiate(femalePrefab, RandomPoint(), Quaternion.identity);
            female.name = "Female_" + i;
            RegisterCreature(female, "Female_" + i);
        }

        GameObject historyRecorderGo = new GameObject("PopulationHistoryRecorder");
        historyRecorderGo.AddComponent<PopulationHistoryRecorder>();
    }

    private static Vector2 RandomPoint()
    {
        return new Vector2(Random.Range(-WorldHalfSize, WorldHalfSize), Random.Range(-WorldHalfSize, WorldHalfSize));
    }

    private static void RegisterCreature(GameObject go, string label)
    {
        EnsureComponent<CreatureMemory>(go);
        EnsureComponent<CreatureIdentity>(go);
        EnsureComponent<CreatureHistory>(go);
        EnsureComponent<CreatureGenetics>(go);
        EnsureComponent<CreatureFamily>(go);

        knownCreatureIds.Add(go.GetInstanceID());
        trackers.Add(new CreatureTracker
        {
            health = go.GetComponent<CreatureHealth>(),
            stats = go.GetComponent<CreatureStats>(),
            reproduction = go.GetComponent<CreatureReproduction>(),
            age = go.GetComponent<CreatureAge>(),
            wanderer = go.GetComponent<CreatureWanderer>(),
            label = label
        });
    }

    private static void EnsureComponent<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() == null)
        {
            go.AddComponent<T>();
        }
    }

    private static void ScanForNewCreatures()
    {
        CreatureHealth[] all = Object.FindObjectsByType<CreatureHealth>(FindObjectsSortMode.None);
        foreach (CreatureHealth h in all)
        {
            int id = h.gameObject.GetInstanceID();
            if (!knownCreatureIds.Contains(id))
            {
                totalBirths++;
                RegisterCreature(h.gameObject, "Born_" + totalBirths);
            }
        }
    }

    private static void ReportInterval()
    {
        int population = 0;
        float foodSum = 0f;
        float waterSum = 0f;
        float ageSum = 0f;
        int adults = 0;
        int matingReady = 0;
        int diedSoFar = 0;

        totalMatingAttempts = 0;

        foreach (CreatureTracker t in trackers)
        {
            if (t.wanderer != null)
            {
                totalMatingAttempts += t.wanderer.MateSeekAttempts;
            }

            bool dead = t.health != null && t.health.IsDead;
            if (dead)
            {
                diedSoFar++;
                continue;
            }

            population++;

            if (t.stats != null)
            {
                foodSum += t.stats.Food;
                waterSum += t.stats.Water;
            }

            bool isAdult = t.age != null && t.age.CurrentStage == CreatureAge.Stage.Adult;
            if (t.age != null)
            {
                ageSum += t.age.Age;
                if (isAdult)
                {
                    adults++;
                }
            }

            if (isAdult && t.reproduction != null && t.reproduction.IsAvailableToMate && t.reproduction.IsContent())
            {
                matingReady++;
            }

            if (t.reproduction != null)
            {
                bool availableNow = t.reproduction.IsAvailableToMate;
                if (t.wasAvailable && !availableNow)
                {
                    totalBonds++;
                }
                t.wasAvailable = availableNow;
            }
        }

        Debug.Log(string.Format(
            "[t={0:F0}s] population={1} avgFood={2:F1} avgWater={3:F1} avgAge={4:F1} adults={5} matingReady={6} diedSoFar={7} bonds={8} births={9} mateAttempts={10}",
            Time.time, population,
            population > 0 ? foodSum / population : 0f,
            population > 0 ? waterSum / population : 0f,
            population > 0 ? ageSum / population : 0f,
            adults, matingReady, diedSoFar, totalBonds, totalBirths, totalMatingAttempts));
    }

    private static void ReportFinal(bool hitSafetyTimeout)
    {
        ReportInterval();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== FINAL SIMULATION RESULTS ===");
        if (hitSafetyTimeout)
        {
            sb.AppendLine("(hit real-time safety cap before reaching target sim duration)");
        }
        sb.AppendLine("Simulated game-seconds elapsed: " + Time.time.ToString("F0"));
        sb.AppendLine("Total creatures tracked (starting parents + all births): " + trackers.Count);
        sb.AppendLine("Total births during run: " + totalBirths);
        sb.AppendLine("Total mate-seek attempts: " + totalMatingAttempts);
        sb.AppendLine("Total successful bonds: " + totalBonds);

        int diedCount = 0;
        int reachedAdult = 0;
        float lifespanSum = 0f;
        Dictionary<string, int> reasonCounts = new Dictionary<string, int>();

        foreach (CreatureTracker t in trackers)
        {
            bool everAdult = t.age != null && (int)t.age.CurrentStage >= (int)CreatureAge.Stage.Adult;
            if (everAdult)
            {
                reachedAdult++;
            }

            if (t.health != null && t.health.IsDead)
            {
                diedCount++;
                lifespanSum += t.age != null ? t.age.Age : 0f;

                string reason = t.health.DeathReason;
                if (!reasonCounts.ContainsKey(reason))
                {
                    reasonCounts[reason] = 0;
                }
                reasonCounts[reason]++;
            }
        }

        sb.AppendLine("Died: " + diedCount + " / " + trackers.Count);
        sb.AppendLine("Average lifespan of the dead (sim-seconds): " + (diedCount > 0 ? (lifespanSum / diedCount).ToString("F1") : "n/a"));
        sb.AppendLine("Reached Adult stage at some point: " + reachedAdult + " / " + trackers.Count);
        sb.AppendLine("Death reason breakdown:");
        foreach (KeyValuePair<string, int> kv in reasonCounts)
        {
            sb.AppendLine("  " + kv.Key + ": " + kv.Value);
        }

        Debug.Log(sb.ToString());

        LogPopulationHistory();
    }

    private static void LogPopulationHistory()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== POPULATION HISTORY ===");
        sb.AppendLine("time\tpop\tadults\tjuv\tbabies\told\tavgFood\tavgWater\tbirths\tdeaths");

        foreach (PopulationSnapshot s in PopulationHistory.Snapshots)
        {
            sb.AppendLine(string.Format(
                "{0:F0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6:F1}\t{7:F1}\t{8}\t{9}",
                s.Time, s.Population, s.Adults, s.Juveniles, s.Babies, s.Old,
                s.AverageFood, s.AverageWater, s.BirthsThisInterval, s.DeathsThisInterval));
        }

        Debug.Log(sb.ToString());
    }
}
