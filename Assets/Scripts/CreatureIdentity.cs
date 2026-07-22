using UnityEngine;

public class CreatureIdentity : MonoBehaviour
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    private void Awake()
    {
        Id = CreatureRegistry.TakeNextId();
        Name = NameGenerator.Generate();
        CreatureRegistry.Register(this);
    }

    private void OnDestroy()
    {
        CreatureRegistry.Unregister(Id);
    }

    public string DisplayLabel()
    {
        return Name + " #" + Id.ToString("D5");
    }
}
