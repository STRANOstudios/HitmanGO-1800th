using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectCubes : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform cube;
    public Transform cube2;

    public Material mat;

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

        // Draw line to the screen center
        //GL.Begin(GL.LINES);
        //GL.Color(Color.red);
        //GL.Vertex(Vector2.zero);
        //GL.Vertex(Vector2.one * .5f);
        //GL.End();

        //GL.Begin(GL.LINES);
        //GL.Color(Color.red);
        //GL.Vertex(new Vector2(0.5f, 0f));
        //GL.Vertex(new Vector2(0.5f, 1f));
        //GL.End();


        Vector2 start = Camera.main.WorldToScreenPoint(cube.position);
        Vector2 end = Camera.main.WorldToScreenPoint(cube2.position);

        float space = 5;
        float dash = 10;

        Vector2 direction = end - start;
        int dashes = (int)(direction.magnitude / (dash + space));
        direction = direction.normalized;

        for (int i = 0; i < dashes; i++)
        {
            Vector2 offset = direction * (dash + space) * i;
            //drawLine(start + offset, start + offset + direction * dash);
        }

        GL.PopMatrix();
    }

    void drawLine(Vector2 start, Vector2 end)
    {
        GL.Begin(GL.QUADS);
        GL.Color(Color.red);
        Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * width;

        GL.Vertex(glVector(start - offset));
        GL.Vertex(glVector(start + offset));
        GL.Vertex(glVector(end + offset));
        GL.Vertex(glVector(end - offset));
        GL.End();
    }

    public Vector2 glVector(Vector2 screen)
    {
        return new Vector2(screen.x / Screen.width, screen.y / Screen.height);
    }

    public float width = 1;

    public void Update()
    {
        LineDrawer_GL.addWorldLine(Color.cyan, width, cube.position, cube2.position, 5, 10);
    }
}