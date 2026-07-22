using System.Collections.Generic;
using System.Linq;

public static class PopulationStats
{
    private static readonly Dictionary<string, int> deathsByReason = new Dictionary<string, int>();
    private static readonly Dictionary<int, int> lineageCounts = new Dictionary<int, int>();
    private static readonly Dictionary<int, string> lineageNames = new Dictionary<int, string>();

    public static int TotalBirths { get; private set; }
    public static int TotalDeaths { get; private set; }

    public static void RecordDeath(string reason)
    {
        TotalDeaths++;

        if (!deathsByReason.ContainsKey(reason))
        {
            deathsByReason[reason] = 0;
        }
        deathsByReason[reason]++;
    }

    public static void RecordBirth(int lineageRootId)
    {
        TotalBirths++;

        if (!lineageCounts.ContainsKey(lineageRootId))
        {
            lineageCounts[lineageRootId] = 0;
            CreatureIdentity rootIdentity = CreatureRegistry.Get(lineageRootId);
            lineageNames[lineageRootId] = rootIdentity != null ? rootIdentity.Name : ("#" + lineageRootId.ToString("D5"));
        }
        lineageCounts[lineageRootId]++;
    }

    public static IReadOnlyDictionary<string, int> GetDeathCounts()
    {
        return deathsByReason;
    }

    public static List<(string name, int count)> GetTopLineages(int count)
    {
        return lineageCounts
            .OrderByDescending(kv => kv.Value)
            .Take(count)
            .Select(kv => (lineageNames.TryGetValue(kv.Key, out string name) ? name : "Unknown", kv.Value))
            .ToList();
    }
}
