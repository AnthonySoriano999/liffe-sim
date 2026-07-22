using System.Collections.Generic;

public static class CreatureRegistry
{
    private static int nextId = 1;
    private static readonly Dictionary<int, CreatureIdentity> byId = new Dictionary<int, CreatureIdentity>();

    public static int Count => byId.Count;

    public static int TakeNextId()
    {
        return nextId++;
    }

    public static void Register(CreatureIdentity identity)
    {
        byId[identity.Id] = identity;
    }

    public static void Unregister(int id)
    {
        byId.Remove(id);
    }

    public static CreatureIdentity Get(int id)
    {
        byId.TryGetValue(id, out CreatureIdentity identity);
        return identity;
    }
}
