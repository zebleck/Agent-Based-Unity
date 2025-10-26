using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class DrawCircle : MonoBehaviour
{
    public int segments = 64;
    public float radius = 2f;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.red;

    private LineRenderer line;
    private Material cachedMaterial;

    void Awake()
    {
        EnsureSetup();
    }

    void OnValidate()
    {
        EnsureSetup();
        UpdateCircle();
    }

    private void EnsureSetup()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        // Create material once if missing
        if (cachedMaterial == null)
        {
            cachedMaterial = new Material(Shader.Find("Unlit/Color"));
            cachedMaterial.hideFlags = HideFlags.HideAndDontSave; // avoid clutter in project
        }

        line.sharedMaterial = cachedMaterial;
        line.sharedMaterial.color = lineColor;

        line.useWorldSpace = false;
        line.loop = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
    }

    private void UpdateCircle()
    {
        if (line == null) return;

        line.positionCount = Mathf.Max(3, segments);

        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            line.SetPosition(i, new Vector3(x, 0, z));
        }
    }
}
