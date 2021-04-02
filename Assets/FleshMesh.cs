using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FleshMesh : MonoBehaviour
{

    public HashSet<MeshTriangle> alienTris = new HashSet<MeshTriangle>();
    public Mesh mesh;
    private MeshFilter filter;
    private MeshRenderer render;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        filter = gameObject.GetComponent<MeshFilter>();
        render = gameObject.GetComponent<MeshRenderer>();
        Material newMat = Resources.Load<Material>("mat");
        render.material = newMat;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMesh();
    }

    void UpdateMesh()
    {
        Vector3[] vertices = new Vector3[alienTris.Count * 3];
        Vector2[] uvs = new Vector2[alienTris.Count * 3];
        int[] triangles = new int[alienTris.Count* 3];

        int index = 0;

        List<MeshTriangle> removalq = new List<MeshTriangle>();

        foreach (MeshTriangle tri in alienTris)
        {
            bool skip = false;

            for (int i = 0; i < 3; i++)
            {
                var obj = tri.Triangles[i];
                var j = i + 1;
                if (j > 2)
                    j = 0;
                if (obj == null || tri.Triangles[j] == null || (obj.transform.position-tri.Triangles[j].transform.position).sqrMagnitude > 64)
                {
                    skip = true;
                    removalq.Add(tri);
                    break;
                }
            }

            if (skip)
                continue;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var obj = tri.Triangles[i];
                    vertices[index] = obj.transform.position;
                    uvs[index] = new Vector2(0, 0);
                    triangles[index] = index;
                }
                catch(Exception e)
                {
                    Debug.Log(e.ToString() + "\n " + index.ToString() + " / " + vertices.Length.ToString());
                }
                index++;
            }
        }

        foreach(var failure in removalq)
        {
            alienTris.Remove(failure);
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        filter.mesh = mesh;
    }
}
