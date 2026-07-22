using UnityEngine;
using System.Collections.Generic;

public enum MemoryType
{
    Food,
    Water,
    Danger,
    Nest,
    Conflict
}

public class MemoryEntry
{
    public Vector2 Position;
    public MemoryType Type;
    public float Strength;
}

public class CreatureMemory : MonoBehaviour
{
    [SerializeField] private int maxMemories = 5;
    [SerializeField] private float initialStrength = 1f;
    [SerializeField] private float decayPerSecond = 0.01f;
    [SerializeField] private float minUsableStrength = 0.15f;
    [SerializeField] private float sameSpotRadius = 1f;
    [SerializeField] private float weakenFactor = 0.3f;

    private readonly List<MemoryEntry> memories = new List<MemoryEntry>();
    private CreatureIdentity identity;

    private void Awake()
    {
        identity = GetComponent<CreatureIdentity>();
    }

    private void Update()
    {
        for (int i = memories.Count - 1; i >= 0; i--)
        {
            memories[i].Strength -= decayPerSecond * Time.deltaTime;
            if (memories[i].Strength <= 0f)
            {
                memories.RemoveAt(i);
            }
        }
    }

    public void Record(MemoryType type, Vector2 position)
    {
        string name = identity != null ? identity.Name : gameObject.name;

        MemoryEntry existing = FindNear(type, position);
        if (existing != null)
        {
            existing.Strength = initialStrength;
            existing.Position = position;
            Debug.Log(name + ": Memory Reinforced: " + type + " at " + FormatPosition(position));
            return;
        }

        if (memories.Count >= maxMemories)
        {
            EvictWeakest();
        }

        memories.Add(new MemoryEntry { Type = type, Position = position, Strength = initialStrength });
        Debug.Log(name + ": Memory Created: " + type + " at " + FormatPosition(position));
    }

    private string FormatPosition(Vector2 position)
    {
        return "(" + position.x.ToString("F1") + ", " + position.y.ToString("F1") + ")";
    }

    public bool TryRecall(MemoryType type, out Vector2 position)
    {
        MemoryEntry best = null;
        foreach (MemoryEntry m in memories)
        {
            if (m.Type != type || m.Strength < minUsableStrength)
            {
                continue;
            }
            if (best == null || m.Strength > best.Strength)
            {
                best = m;
            }
        }

        if (best == null || Random.value > best.Strength)
        {
            position = Vector2.zero;
            return false;
        }

        position = best.Position;
        return true;
    }

    public void Weaken(MemoryType type, Vector2 position)
    {
        MemoryEntry match = FindNear(type, position);
        if (match != null)
        {
            match.Strength *= weakenFactor;
        }
    }

    private MemoryEntry FindNear(MemoryType type, Vector2 position)
    {
        foreach (MemoryEntry m in memories)
        {
            if (m.Type == type && Vector2.Distance(m.Position, position) < sameSpotRadius)
            {
                return m;
            }
        }
        return null;
    }

    private void EvictWeakest()
    {
        MemoryEntry weakest = null;
        foreach (MemoryEntry m in memories)
        {
            if (weakest == null || m.Strength < weakest.Strength)
            {
                weakest = m;
            }
        }

        if (weakest != null)
        {
            memories.Remove(weakest);
        }
    }
}
