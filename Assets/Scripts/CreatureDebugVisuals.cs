using UnityEngine;

public class CreatureDebugVisuals : MonoBehaviour
{
    [SerializeField] private int circleSegments = 32;
    [SerializeField] private Color visionColor = new Color(0f, 1f, 1f, 0.4f);
    [SerializeField] private Color seeingColor = Color.green;
    [SerializeField] private Color rememberingColor = Color.yellow;
    [SerializeField] private Color followingColor = new Color(0.3f, 0.5f, 1f);

    private LineRenderer visionCircle;
    private LineRenderer targetLine;
    private CreatureVision vision;
    private CreatureWanderer wanderer;
    private bool visible;

    private void Awake()
    {
        vision = GetComponent<CreatureVision>();
        wanderer = GetComponent<CreatureWanderer>();

        visionCircle = CreateLineRenderer("VisionCircleGizmo", visionColor, 0.05f, true);
        targetLine = CreateLineRenderer("TargetLineGizmo", seeingColor, 0.06f, false);

        SetVisible(false);
    }

    private LineRenderer CreateLineRenderer(string name, Color color, float width, bool loop)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.loop = loop;
        lr.useWorldSpace = true;
        lr.sortingOrder = 10;

        return lr;
    }

    public void SetVisible(bool value)
    {
        visible = value;
        visionCircle.enabled = value;
        targetLine.enabled = value;
    }

    private void Update()
    {
        if (!visible)
        {
            return;
        }

        DrawVisionCircle();
        DrawTargetLine();
    }

    private void DrawVisionCircle()
    {
        float radius = vision != null ? vision.EffectiveVisionRadius : 0f;
        visionCircle.positionCount = circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (float)i / circleSegments * Mathf.PI * 2f;
            Vector3 point = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            visionCircle.SetPosition(i, point);
        }
    }

    private void DrawTargetLine()
    {
        if (wanderer == null || wanderer.CurrentTargetKind == CreatureWanderer.TargetKind.None)
        {
            targetLine.positionCount = 0;
            return;
        }

        Color color = seeingColor;
        if (wanderer.CurrentTargetKind == CreatureWanderer.TargetKind.Remembering)
        {
            color = rememberingColor;
        }
        else if (wanderer.CurrentTargetKind == CreatureWanderer.TargetKind.FollowingParent)
        {
            color = followingColor;
        }

        targetLine.startColor = color;
        targetLine.endColor = color;
        targetLine.positionCount = 2;
        targetLine.SetPosition(0, transform.position);
        targetLine.SetPosition(1, new Vector3(wanderer.CurrentTargetPosition.x, wanderer.CurrentTargetPosition.y, transform.position.z));
    }
}
