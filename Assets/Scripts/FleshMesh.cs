using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using System.Threading.Tasks;
using Unity.Profiling;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Jobs;
using MathU = Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

public class FleshMesh : MonoBehaviour
{
    
    //public NativeList<MeshTriangle> hexSets = new NativeList<MeshTriangle>();
    public Mesh mesh;
    private MeshFilter filter;
    private MeshRenderer render;
    private TransformAccessArray transformsAccess;
    private int sin = 0;
    private bool initd = false;

    public void InitializeTransforms(List<MeshTriangle> tris)
    {
        var vertTransforms = new Transform[tris.Count*3];
        int k = 0;

        for(int i = 0; i < tris.Count; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                Debug.Log($"k: {k} / {vertTransforms.Length}, i: {i} / {tris.Count}, j: {j} ");
                vertTransforms[k] = tris[i].Hexes[j];
                k++;
            }
        }
        sin = tris.Count * 3;
        transformsAccess = new TransformAccessArray(vertTransforms);
        initd = true;
    }

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
        if (!initd)
            return;

        UpdateMesh();

    }

    unsafe void UpdateMesh()
    {


       // var removalIndices = new NativeList<int>(Allocator.TempJob);
        var positions = new NativeArray<ValidatedPos>(sin, Allocator.TempJob);
        var finpositions = new NativeList<MathU.float3>(sin, Allocator.TempJob);

        var uvs = new NativeArray<MathU.float2>(sin, Allocator.TempJob);
        var verts = new NativeArray<MathU.float3>(sin, Allocator.TempJob);
        var tris = new NativeArray<int>(sin, Allocator.TempJob);
        var posSet = new NativeArray<MathU.float3>(3, Allocator.TempJob);

        JobHandle transformCopyHandle = new TransformCopy()
        {

            Positions = positions,

        }.Schedule(transformsAccess);

        transformCopyHandle.Complete();

        JobHandle getFloatJobHandle = new DisqualifyJob()//FloatJob()
        {

            ValidatedPositions = positions,
            PosSet = posSet,
            FinalPositions = finpositions

        }.Schedule(sin,transformCopyHandle);

        getFloatJobHandle.Complete();

        JobHandle vertJobHandle = new VertJob()
        {

            UVs = uvs,
            Triangles = tris,
            Positions = finpositions,
            Vertices = verts

        }.Schedule(finpositions.Length, 32, getFloatJobHandle);

        vertJobHandle.Complete();
        //removalIndices.Dispose();

        var marker = new ProfilerMarker("Mesh Apply");
        marker.Begin();

        mesh.Clear();
        mesh.vertices = verts.Reinterpret<Vector3>(12).ToArray();
        mesh.uv = uvs.Reinterpret<Vector2>(8).ToArray();
        mesh.triangles = tris.Reinterpret<int>(4).ToArray();
        filter.mesh = mesh;
        marker.End();

        positions.Dispose();
        finpositions.Dispose();
        //posSet.Dispose();
        uvs.Dispose();
        verts.Dispose();
        tris.Dispose();

    }
}

[BurstCompile]
public struct DisqualifyJob : IJobFor
{
    public NativeArray<ValidatedPos> ValidatedPositions;
    public NativeList<MathU.float3> FinalPositions;
    [DeallocateOnJobCompletion]
    public NativeArray<MathU.float3> PosSet; // initialise this to a size of 3

    private bool setIsValid;

    public void Execute(int index)
    {
        var validPos = ValidatedPositions[index];
        int indexInSet = index % 3;
        setIsValid |= indexInSet == 0;
        if (!setIsValid)
        {
            return;
        }

        if (validPos.IsValid)
        {
            PosSet[indexInSet] = validPos.Position;
            if (indexInSet == 2 && SetIsCloseEnough())
            {
                FinalPositions.AddRange(PosSet);
            }
        }
        else
        {
            setIsValid = false;
        }
    }

    private bool SetIsCloseEnough()
    {
        for (int i = 0; i < 3; i++)
        {
            int n = (i + 1) % 3;
            if (MathU.math.distancesq(PosSet[i], PosSet[n]) > 64)
            {
                return false;
            }
        }

        return true;
    }
}

public struct ValidatedPos
{
    public MathU.float3 Position;
    public bool IsValid;

    public ValidatedPos(MathU.float3 position, bool isValid)
    {
        Position = position;
        IsValid = isValid;
    }
}

[BurstCompile]
public struct TransformCopy : IJobParallelForTransform
{
    public NativeArray<ValidatedPos> Positions;

    public void Execute(int index, TransformAccess transform)
    {
        Positions[index] = new ValidatedPos(transform.position, transform.isValid);
    }
}

[BurstCompile] // <- magic fast maker
public struct VertJob : IJobParallelFor
{
    public NativeArray<MathU.float2> UVs;
    public NativeArray<int> Triangles;
    [ReadOnly]
    public NativeList<MathU.float3> Positions;
    public NativeArray<MathU.float3> Vertices;

    public void Execute(int index)
    {
        Vertices[index] = Positions[index];
        UVs[index] = new MathU.float2(0f, 0f);
        Triangles[index] = index;
    }
}









/*

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
*/