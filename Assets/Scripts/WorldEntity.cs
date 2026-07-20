using UnityEngine;

public enum WorldEntityType
{
    Food,
    Water,
    Mate
}

public class WorldEntity : MonoBehaviour
{
    public WorldEntityType entityType;
}
