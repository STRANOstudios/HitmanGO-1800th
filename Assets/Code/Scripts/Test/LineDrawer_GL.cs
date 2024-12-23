using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer_GL : MonoBehaviour
{
    public class Line_GL
    {
        public Line_GL(Color color, float width, Vector2 start, Vector2 end)
        {
            this.color = color;
            this.width = width;
            this.start = start;
            this.end = end;
        }
        public Vector2 start;
        public Vector2 end;
        public Color color;
        public float width;

        public float dashLength = 0;
        public float spaceLength = 0;

        public Line_GL setDashedLine(float dashLength, float spaceLength)
        {
            if (dashLength < 1 || spaceLength < 1)
                return this;
            this.dashLength = dashLength;
            this.spaceLength = spaceLength;
            return this;
        }

    }

    public static LineDrawer_GL instance;

    private static List<Line_GL> drawLines = new List<Line_GL>();

    // Start is called before the first frame update
    public Material mat;
    private Camera main;
    void Start()
    {
        //Check if on camera
        main = GetComponent<Camera>();
        if (main == null)
        {
            Debug.LogError("Error - parent != Camera");
            Destroy(this);
            return;
        }
        instance = this;
    }

    void OnPostRender()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();


        foreach (Line_GL line in drawLines)
        {
            if (line.dashLength > 0)
            {
                drawDashedLine(line.color, line.width, line.start, line.end, line.dashLength, line.spaceLength);
            }
            else
            {
                drawLine(line.color, line.width, line.start, line.end);
            }
        }

        //Draw

        GL.PopMatrix();
        drawLines = new List<Line_GL>();
    }

    //world position to screen position
    public static Vector2 worldToScreenPos(Vector3 worldPosition)
    {
        return instance.main.WorldToScreenPoint(worldPosition);
    }

    //screen position to GL position
    private static Vector2 screenToGlVertex(Vector2 vec)
    {
        return new Vector2(vec.x / Screen.width, vec.y / Screen.height);
    }

    //Draw line with width of 1
    void drawLine(Color color, Vector2 start, Vector2 end)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);
        GL.Vertex(screenToGlVertex(start));
        GL.Vertex(screenToGlVertex(end));
        GL.End();
    }

    //Draw a Wide line (Quard)
    void drawLine(Color color, float lineWidth, Vector2 start, Vector2 end)
    {
        if (lineWidth < 2f)
        {
            drawLine(color, start, end);
        }
        else
        {
            float thisWidth = lineWidth * 0.25f;

            GL.Begin(GL.QUADS);
            GL.Color(color);
            //get the perpandicular vector
            Vector2 perpendicular = (new Vector2(end.y, start.x) - new Vector2(start.y, end.x)).normalized * thisWidth;

            //create the 4 vertexes of the quad
            GL.Vertex(screenToGlVertex(start - perpendicular));
            GL.Vertex(screenToGlVertex(start + perpendicular));
            GL.Vertex(screenToGlVertex(end + perpendicular));
            GL.Vertex(screenToGlVertex(end - perpendicular));
            GL.End();
        }
    }

    void drawDashedLine(Color color, float lineWidth, Vector2 start, Vector2 end, float dashLength, float space)
    {
        Vector2 direction = (end - start);
        float distance = direction.magnitude;
        direction = direction.normalized;

        int times = (int)(distance / (dashLength + space));
        for (int i = 0; i < times; i++)
        {
            Vector2 offset = direction * (space + dashLength) * i;
            Vector2 begin = offset + start;

            drawLine(color, lineWidth, begin, begin + direction * dashLength);
        }
    }

    //draw lines on screen cordinates
    public static void addLine(Color color, Vector2 start, Vector2 end)
    {
        drawLines.Add(new Line_GL(color, 1, start, end));
    }

    public static void addLine(Color color, float width, Vector2 start, Vector2 end)
    {
        drawLines.Add(new Line_GL(color, width, start, end));
    }

    public static void addLine(Color color, float width, Vector2 start, Vector2 end, float dashLength, float spaceLength)
    {
        drawLines.Add(new Line_GL(color, width, start, end).setDashedLine(dashLength, spaceLength));
    }

    public static void addLine(Line_GL line)
    {
        drawLines.Add(line);
    }

    // methods to draw line between World cordinates
    public static void addWorldLine(Color color, Vector3 start, Vector3 end)
    {
        drawLines.Add(new Line_GL(color, 1, worldToScreenPos(start), worldToScreenPos(end)));
    }

    public static void addWorldLine(Color color, float width, Vector3 start, Vector3 end)
    {
        drawLines.Add(new Line_GL(color, width, worldToScreenPos(start), worldToScreenPos(end)));
    }

    public static void addWorldLine(Color color, float width, Vector3 start, Vector3 end, float dashLength, float spaceLength)
    {
        drawLines.Add(new Line_GL(color, width, worldToScreenPos(start), worldToScreenPos(end)).setDashedLine(dashLength, spaceLength));
    }
}