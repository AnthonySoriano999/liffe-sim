using UnityEngine;

public class PopulationHistoryRecorder : MonoBehaviour
{
    [SerializeField] private float sampleInterval = 20f;

    private float timer;
    private int lastTotalBirths;
    private int lastTotalDeaths;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < sampleInterval)
        {
            return;
        }

        timer = 0f;
        Sample();
    }

    private void Sample()
    {
        CreatureHealth[] all = Object.FindObjectsByType<CreatureHealth>(FindObjectsSortMode.None);

        int population = 0;
        int adults = 0;
        int juveniles = 0;
        int babies = 0;
        int old = 0;
        float foodSum = 0f;
        float waterSum = 0f;

        foreach (CreatureHealth h in all)
        {
            if (h.IsDead)
            {
                continue;
            }

            population++;

            CreatureAge age = h.GetComponent<CreatureAge>();
            if (age != null)
            {
                if (age.CurrentStage == CreatureAge.Stage.Adult)
                {
                    adults++;
                }
                else if (age.CurrentStage == CreatureAge.Stage.Juvenile)
                {
                    juveniles++;
                }
                else if (age.CurrentStage == CreatureAge.Stage.Baby)
                {
                    babies++;
                }
                else if (age.CurrentStage == CreatureAge.Stage.Old)
                {
                    old++;
                }
            }

            CreatureStats stats = h.GetComponent<CreatureStats>();
            if (stats != null)
            {
                foodSum += stats.Food;
                waterSum += stats.Water;
            }
        }

        int birthsThisInterval = PopulationStats.TotalBirths - lastTotalBirths;
        int deathsThisInterval = PopulationStats.TotalDeaths - lastTotalDeaths;
        lastTotalBirths = PopulationStats.TotalBirths;
        lastTotalDeaths = PopulationStats.TotalDeaths;

        PopulationHistory.Record(new PopulationSnapshot
        {
            Time = Time.time,
            Population = population,
            Adults = adults,
            Juveniles = juveniles,
            Babies = babies,
            Old = old,
            AverageFood = population > 0 ? foodSum / population : 0f,
            AverageWater = population > 0 ? waterSum / population : 0f,
            BirthsThisInterval = birthsThisInterval,
            DeathsThisInterval = deathsThisInterval
        });
    }
}
