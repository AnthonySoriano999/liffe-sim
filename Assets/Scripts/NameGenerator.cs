using UnityEngine;

public static class NameGenerator
{
    private static readonly string[] Consonants = { "K", "V", "M", "T", "R", "S", "N", "L", "D", "B", "F", "G", "P", "Z", "Y" };
    private static readonly string[] Vowels = { "a", "e", "i", "o", "u" };

    public static string Generate()
    {
        string first = Consonants[Random.Range(0, Consonants.Length)] + Vowels[Random.Range(0, Vowels.Length)];
        string second = Consonants[Random.Range(0, Consonants.Length)].ToLower() + Vowels[Random.Range(0, Vowels.Length)];
        return first + second;
    }
}
