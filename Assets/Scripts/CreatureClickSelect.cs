using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CreatureStats))]
[RequireComponent(typeof(Collider2D))]
public class CreatureClickSelect : MonoBehaviour
{
    private CreatureStats stats;
    private Collider2D col;

    private void Awake()
    {
        stats = GetComponent<CreatureStats>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        if (col.OverlapPoint(worldPos))
        {
            CreatureStatsPanel.Instance.ShowCreature(stats);
        }
    }
}
