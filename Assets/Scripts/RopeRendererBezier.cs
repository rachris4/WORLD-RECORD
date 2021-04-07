using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RopeRendererBezier : MonoBehaviour
{
    [SerializeField]
    public float steps = 20f;
    [SerializeField]
    public float maxLength = 10f;

    public GameObject hook;
    public GameObject shooter;

    private float t = 0f;
    private Bezier myBezier;
    private LineRenderer lineRenderer;
    private float width = 0.2f;

    private Vector3 h2old = Vector3.zero;
    private Vector3 h3old = Vector3.zero;

    public void Initialize(GameObject start, GameObject end)
    {
        hook = start;
        shooter = end;
        var h1 = hook.transform.position;
        var h4 = shooter.transform.position;
        var h2 = Vector3.Normalize(h4 - h1) + h1;
        var h3 = Vector3.Normalize(h1 - h4) + h4;
        myBezier = new Bezier(h1,h4,h1,h4,2f);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        Material newMat = Resources.Load<Material>("mat");
        lineRenderer.material = newMat;
        lineRenderer.enabled = true;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    void Update()
    {
        if (hook == null || shooter == null)
            return;
        else if (lineRenderer == null)
        {
            Initialize(hook, shooter);
        }

        var h1 = hook.transform.position;
        var h4 = shooter.transform.position;
        var h2 = h4 - h1;
        //h2.y = 0f;
        h2 = Vector3.Normalize(h2);
        var h3 = shooter.transform.rotation * new Vector3(2f, 0f, 0f);
        var grav = maxLength / ((h1 - h4).magnitude+1f);
        if (grav < 0)
            grav = 0;
        else if (grav > maxLength / 2)
            grav = maxLength / 2;
        myBezier = new Bezier(h1, (h2+h2old)/2f, (h3+h3old)/2f, h4, grav);

        h2old = (h2+4*h2old)/ 5f;
        h3old = (h3 +4*h3old) / 5f;
        var pointList = new List<Vector3>();

        for (t = 0; t <= 1; t += 1f/steps)
        {
            Vector3 vec = myBezier.GetPointAtTime(t);
            pointList.Add(vec);
        }
        lineRenderer.positionCount = pointList.Count;
        lineRenderer.SetPositions(pointList.ToArray());
    }
}

[System.Serializable]
public class Bezier
{

    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public float ti = 0f;

    private Vector3 b0 = Vector3.zero;
    private Vector3 b1 = Vector3.zero;
    private Vector3 b2 = Vector3.zero;
    private Vector3 b3 = Vector3.zero;

    private float Ax;
    private float Ay;
    private float Az;

    private float Bx;
    private float By;
    private float Bz;

    private float Cx;
    private float Cy;
    private float Cz;

    private float gravity;

    // Init function v0 = 1st point, v1 = handle of the 1st point , v2 = handle of the 2nd point, v3 = 2nd point
    // handle1 = v0 + v1
    // handle2 = v3 + v2
    public Bezier(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float grav = 0f)
    {
        p0 = v0;
        p1 = v1 - new Vector3(0f,1f,0f)*grav;
        p2 = v2 - new Vector3(0f, 1f, 0f) * grav;
        p3 = v3;
        gravity = grav;
    }

    // 0.0 >= t <= 1.0
    public Vector3 GetPointAtTime(float t)
    {
        CheckConstant();
        float t2 = t * t;
        float t3 = t * t * t;
        float x = Ax * t3 + Bx * t2 + Cx * t + p0.x;
        float y = Ay * t3 + By * t2 + Cy * t + p0.y;
        float z = 0f;//Az * t3 + Bz * t2 + Cz * t + p0.z;
        return new Vector3(x, y, z);

    }

    private void SetConstant()
    {
        Cx = 3f * ((p0.x + p1.x) - p0.x);
        Bx = 3f * ((p3.x + p2.x) - (p0.x + p1.x)) - Cx;
        Ax = p3.x - p0.x - Cx - Bx;

        Cy = 3f * ((p0.y + p1.y) - p0.y);
        By = 3f * ((p3.y + p2.y) - (p0.y + p1.y)) - Cy;
        Ay = p3.y - p0.y - Cy - By;

        Cz = 3f * ((p0.z + p1.z) - p0.z);
        Bz = 3f * ((p3.z + p2.z) - (p0.z + p1.z)) - Cz;
        Az = p3.z - p0.z - Cz - Bz;

    }

    // Check if p0, p1, p2 or p3 have changed
    private void CheckConstant()
    {
        if (p0 != b0 || p1 != b1 || p2 != b2 || p3 != b3)
        {
            SetConstant();
            b0 = p0;
            b1 = p1;
            b2 = p2;
            b3 = p3;
        }
    }
}