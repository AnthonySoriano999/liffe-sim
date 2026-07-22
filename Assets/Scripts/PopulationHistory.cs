using System.Collections.Generic;

public struct PopulationSnapshot
{
    public float Time;
    public int Population;
    public int Adults;
    public int Juveniles;
    public int Babies;
    public int Old;
    public float AverageFood;
    public float AverageWater;
    public int BirthsThisInterval;
    public int DeathsThisInterval;
}

public static class PopulationHistory
{
    private static readonly List<PopulationSnapshot> snapshots = new List<PopulationSnapshot>();

    public static void Record(PopulationSnapshot snapshot)
    {
        snapshots.Add(snapshot);
    }

    public static IReadOnlyList<PopulationSnapshot> Snapshots => snapshots;

    public static void Clear()
    {
        snapshots.Clear();
    }
}
