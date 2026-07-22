// Failsafe only, not a core ecosystem mechanic. Safe to delete this file and
// its single call site in CreatureReproduction.TryBondWith to remove entirely.
public static class PopulationCap
{
    public static int MaxPopulation = 35;

    public static bool IsAtCapacity()
    {
        return CreatureRegistry.Count >= MaxPopulation;
    }
}
