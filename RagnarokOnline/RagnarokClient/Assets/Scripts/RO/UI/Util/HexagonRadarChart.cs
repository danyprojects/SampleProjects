using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HexagonRadarChart : Graphic
{
    private static readonly Vector2[] VERTICES = new Vector2[7]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(Mathf.Sqrt(3)/2f, 0.5f),
        new Vector2(Mathf.Sqrt(3)/2f, - 0.5f),
        new Vector2(0, -1),
        new Vector2(-Mathf.Sqrt(3)/2f, - 0.5f),
        new Vector2(-Mathf.Sqrt(3)/2f, 0.5f)
    };

    public enum Indexes
    {
        Top,
        TopRight,
        BottomRight,
        Bottom,
        BottomLeft,
        TopLeft
    }

    [SerializeField] private float chartMaxValue = 1f;
    [SerializeField] private float chartDefaultValue = 1f;

    private float[] chartValues;


    public HexagonRadarChart()
    {
        base.Start();
        chartValues = new float[7] {chartDefaultValue , chartDefaultValue , chartDefaultValue , chartDefaultValue,
                                    chartDefaultValue , chartDefaultValue , chartDefaultValue };
    }

    public void setRadarValue(Indexes index, float value)
    {
        chartValues[(int)index + 1] = value;
    }

    public void redrawRadarChart()
    {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        UIVertex vert = UIVertex.simpleVert;
        var size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        size /= chartMaxValue;
        for (int i = 0; i < VERTICES.Length; i++)
        {
            vert.position = VERTICES[i] * size * chartValues[i];
            vert.color = color;
            vh.AddVert(vert);
        }

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
        vh.AddTriangle(0, 3, 4);
        vh.AddTriangle(0, 4, 5);
        vh.AddTriangle(0, 5, 6);
        vh.AddTriangle(0, 6, 1);
    }
}