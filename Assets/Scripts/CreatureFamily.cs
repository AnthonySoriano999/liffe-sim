using System.Collections.Generic;
using UnityEngine;

public class CreatureFamily : MonoBehaviour
{
    public int MotherId { get; private set; } = -1;
    public string MotherName { get; private set; } = "";
    public int FatherId { get; private set; } = -1;
    public string FatherName { get; private set; } = "";
    public int LineageRootId { get; private set; } = -1;
    public IReadOnlyList<int> OffspringIds => offspringIds;

    private readonly List<int> offspringIds = new List<int>();

    public void SetParents(int motherId, string motherName, int fatherId, string fatherName)
    {
        MotherId = motherId;
        MotherName = motherName;
        FatherId = fatherId;
        FatherName = fatherName;
    }

    public void SetLineageRoot(int rootId)
    {
        LineageRootId = rootId;
    }

    public void AddOffspring(int childId)
    {
        offspringIds.Add(childId);
    }

    public List<int> GetSiblingIds()
    {
        List<int> siblings = new List<int>();

        CreatureIdentity selfIdentity = GetComponent<CreatureIdentity>();
        CreatureIdentity mother = MotherId >= 0 ? CreatureRegistry.Get(MotherId) : null;
        if (selfIdentity == null || mother == null)
        {
            return siblings;
        }

        CreatureFamily motherFamily = mother.GetComponent<CreatureFamily>();
        if (motherFamily == null)
        {
            return siblings;
        }

        foreach (int id in motherFamily.OffspringIds)
        {
            if (id != selfIdentity.Id)
            {
                siblings.Add(id);
            }
        }

        return siblings;
    }
}
