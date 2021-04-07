using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

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
        render.sortingOrder = -30;
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

        Debug.Log(alienTris.Count.ToString());

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
                    vertices[index] = obj.transform.position - new Vector3(0f,0f,-0.1f);
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

[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererSortingEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshRenderer renderer = target as MeshRenderer;


        var layers = SortingLayer.layers;

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        int newId = DrawSortingLayersPopup(renderer.sortingLayerID);
        if (EditorGUI.EndChangeCheck())
        {
            renderer.sortingLayerID = newId;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        int order = EditorGUILayout.IntField("Sorting Order", renderer.sortingOrder);
        if (EditorGUI.EndChangeCheck())
        {
            renderer.sortingOrder = order;
        }
        EditorGUILayout.EndHorizontal();

    }

    int DrawSortingLayersPopup(int layerID)
    {
        var layers = SortingLayer.layers;
        var names = layers.Select(l => l.name).ToArray();
        if (!SortingLayer.IsValid(layerID))
        {
            layerID = layers[0].id;
        }
        var layerValue = SortingLayer.GetLayerValueFromID(layerID);
        var newLayerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, names);
        return layers[newLayerValue].id;
    }

}
