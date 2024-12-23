using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLConnect : MonoBehaviour
{
    public Vector3 NodeA = Vector3.zero;
    public Vector3 NodeB = Vector3.zero;
    public Color color = Color.cyan;
    public float width = 1;
    public Material mat;

    void OnPostRender()
    {
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();

        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex(new Vector3(0.5f, 0.5f, 0));
        GL.Vertex(new Vector3(0.7f, 0.7f, 0));
        GL.End();

        GL.PopMatrix();
    }
}
