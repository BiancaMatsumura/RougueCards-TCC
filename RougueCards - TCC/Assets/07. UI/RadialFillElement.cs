using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class RadialFillElement : VisualElement
{
    private float m_Progress = 1f;
    private Texture2D m_Texture;
    private const float StartAngle = 90f;
    private const bool Clockwise = true;

    [UxmlAttribute]
    public float progress
    {
        get => m_Progress;
        set { m_Progress = Mathf.Clamp01(value); MarkDirtyRepaint(); }
    }

    public Texture2D fillTexture
    {
        get => m_Texture;
        set { m_Texture = value; MarkDirtyRepaint(); }
    }

    public RadialFillElement()
    {
        generateVisualContent += OnGenerateVisualContent;
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        if (m_Texture == null || m_Progress <= 0f) return;

        var rect = contentRect;
        if (rect.width < 1f || rect.height < 1f) return;

        Vector2 center = new Vector2(rect.width / 2f, rect.height / 2f);
        float radius = Mathf.Min(rect.width, rect.height) / 2f;

        int segments = Mathf.Max(1, Mathf.CeilToInt(64 * m_Progress));
        float totalAngle = 360f * m_Progress;

        var vertices = new Vertex[segments + 2];
        var indices = new ushort[segments * 3];

        vertices[0] = MakeVertex(center, rect);

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = StartAngle - (Clockwise ? 1 : -1) * t * totalAngle;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad));
            Vector2 pos = center + dir * radius;
            vertices[i + 1] = MakeVertex(pos, rect);
        }

        for (int i = 0; i < segments; i++)
        {
            indices[i * 3] = 0;
            indices[i * 3 + 1] = (ushort)(i + 1);
            indices[i * 3 + 2] = (ushort)(i + 2);
        }

        var mesh = mgc.Allocate(vertices.Length, indices.Length, m_Texture);
        mesh.SetAllVertices(vertices);
        mesh.SetAllIndices(indices);
    }

    Vertex MakeVertex(Vector2 pos, Rect rect)
    {
        Vector2 uv = new Vector2(pos.x / rect.width, 1f - pos.y / rect.height);
        return new Vertex
        {
            position = new Vector3(pos.x, pos.y, Vertex.nearZ),
            tint = Color.white,
            uv = uv
        };
    }
}