using System.Collections.Generic;
using UnityEngine;

public static class LineDrawerGL
{
    public class Line
    {
        public Vector2 Start;
        public Vector2 End;
        public Color Color;
        public float Width;
        public float DashLength;
        public float SpaceLength;

        public Line(Vector2 start, Vector2 end, Color color, float width = 1f, float dashLength = 0, float spaceLength = 0)
        {
            Start = start;
            End = end;
            Color = color;
            Width = width;
            DashLength = dashLength;
            SpaceLength = spaceLength;
        }
    }

    private static readonly List<Line> lines = new();
    private static Material lineMaterial;

    /// <summary>
    /// Aggiunge una linea alla lista di rendering.
    /// </summary>
    public static void AddLine(Vector2 start, Vector2 end, Color color, float width = 1f, float dashLength = 0, float spaceLength = 0)
    {
        lines.Add(new Line(start, end, color, width, dashLength, spaceLength));
    }

    /// <summary>
    /// Disegna tutte le linee accumulate.
    /// </summary>
    public static void DrawLines()
    {
        if (lines.Count == 0) return;

        EnsureMaterial();

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();

        foreach (var line in lines)
        {
            if (line.DashLength > 0)
                DrawDashedLine(line);
            else
                DrawSolidLine(line);
        }

        GL.PopMatrix();
        lines.Clear();
    }

    /// <summary>
    /// Crea un materiale per il rendering delle linee.
    /// </summary>
    private static void EnsureMaterial()
    {
        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private static void DrawSolidLine(Line line)
    {
        if (line.Width <= 1f)
        {
            GL.Begin(GL.LINES);
            GL.Color(line.Color);
            GL.Vertex(ScreenToGL(line.Start));
            GL.Vertex(ScreenToGL(line.End));
            GL.End();
        }
        else
        {
            DrawWideLine(line);
        }
    }

    private static void DrawWideLine(Line line)
    {
        Vector2 direction = (line.End - line.Start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * (line.Width / Screen.height);

        GL.Begin(GL.QUADS);
        GL.Color(line.Color);
        GL.Vertex(ScreenToGL(line.Start - perpendicular));
        GL.Vertex(ScreenToGL(line.Start + perpendicular));
        GL.Vertex(ScreenToGL(line.End + perpendicular));
        GL.Vertex(ScreenToGL(line.End - perpendicular));
        GL.End();
    }

    private static void DrawDashedLine(Line line)
    {
        Vector2 direction = (line.End - line.Start).normalized;
        float distance = Vector2.Distance(line.Start, line.End);
        int segmentCount = Mathf.FloorToInt(distance / (line.DashLength + line.SpaceLength));
        Vector2 offset;

        for (int i = 0; i < segmentCount; i++)
        {
            offset = direction * (line.DashLength + line.SpaceLength) * i;
            Vector2 segmentStart = line.Start + offset;
            Vector2 segmentEnd = segmentStart + direction * line.DashLength;

            DrawSolidLine(new Line(segmentStart, segmentEnd, line.Color, line.Width));
        }
    }

    private static Vector2 ScreenToGL(Vector2 position)
    {
        return new Vector2(position.x / Screen.width, position.y / Screen.height);
    }
}
