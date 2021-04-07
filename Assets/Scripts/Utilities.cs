using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    static public T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
    }

    public static Rigidbody2D FindRigidbody(GameObject obj)
    {
        var rigid = obj.GetComponent<Rigidbody2D>();
        if (rigid != null)
            return rigid;
        for(int i = 0; i < 5; i++)
        {
            if (obj.transform.parent == null)
            {
                return null;
            }
            obj = obj.transform.parent.gameObject;
            rigid = obj.GetComponent<Rigidbody2D>();
            if (rigid != null)
                return rigid;
        }
        return null;
    }

    public static void FindChildRigidBodies(GameObject obj, ref HashSet<Rigidbody2D> result, int limit = 100, int layer = 0)
    {
        layer++;

        foreach (Transform child in obj.transform)
        {
            var rigid = child.gameObject.GetComponent<Rigidbody2D>();
            if (rigid != null)
                result.Add(rigid);
            if(layer < limit)
                FindChildRigidBodies(child.gameObject, ref result,limit,layer);
        }
    }

    public static void RealScale(GameObject obj, ref Vector3 scale)
    {
        var newscale = obj.transform.localScale;
        scale = new Vector3(scale.x * newscale.x, scale.y * newscale.y, scale.z * newscale.z);

        if(obj.transform.parent != null)
        {
            RealScale(obj.transform.parent.gameObject, ref scale);
        }
    }

    public static float RealRotationZFloat(GameObject obj, float rotz)
    {

        Vector3 scaler = new Vector3(1f, 1f, 1f);//obj.transform.localScale;
        RealScale(obj, ref scaler);

        if (scaler.x < 0)
        {
            rotz += 180f;
        }

        return rotz;
    }

    public static Quaternion RealRotation(GameObject obj)
    {

        Vector3 eulerAngs = obj.transform.rotation.eulerAngles;
        float rotz = eulerAngs.z;
        Vector3 scaler = new Vector3(1f, 1f, 1f);//obj.transform.localScale;
        RealScale(obj, ref scaler);

        if(scaler.x < 0)
        {
            rotz += 180f;
        }

        return Quaternion.Euler(eulerAngs.x, eulerAngs.y, rotz);
    }

    public static GameObject FindGameManager()
    {
        return GameObject.Find("GameManager");
    }

    

    public static Vector3 RoundToHexCoordinates(Vector3 convertPosition, float gridSpacing)
    {
        float yspacing = gridSpacing * Mathf.Sqrt(3f);
        float newx1 = Mathf.Round(convertPosition.x / gridSpacing) * gridSpacing;
        float newy1 = Mathf.Round(convertPosition.y / yspacing) * yspacing;

        float newx2 = Mathf.Round(convertPosition.x / gridSpacing) * gridSpacing + gridSpacing / 2;
        float newy2 = Mathf.Round(convertPosition.y / yspacing) * yspacing + yspacing / 2;

        Vector3 result1 = new Vector3(newx1, newy1, 0f);
        Vector3 result2 = new Vector3(newx2, newy2, 0f);
        if (Vector3.Distance(result1, convertPosition) < Vector3.Distance(result2, convertPosition))
        {
            return result1;
        }
        return result2;
    }

    public static Vector3 ConvertToHexagonalCoordinates(Vector3 cartCoord, float gridSpacing)
    {
        gridSpacing /= 2f;

        cartCoord.y *= -1f;

        var q = Mathf.Round((Mathf.Sqrt(3) / 3.5f * cartCoord.x - cartCoord.y/3.5f) / gridSpacing);
        var r = Mathf.Round((2/ 3.5f * cartCoord.y) / gridSpacing);
        var x = q;
        var z = r;
        var y = -x - z;
        return new Vector3(x, y, z);
    }

    public static Vector3[] HexNeighbours(Vector3 hex)
    {
        Vector3[] neighbours = new Vector3[6];

        neighbours[0] = new Vector3(hex.x + 1, hex.y - 1, hex.z + 0);
        neighbours[1] = new Vector3(hex.x + 1, hex.y + 0, hex.z - 1);
        neighbours[2] = new Vector3(hex.x + 0, hex.y + 1, hex.z - 1);
        neighbours[3] = new Vector3(hex.x - 1, hex.y + 1, hex.z + 0);
        neighbours[4] = new Vector3(hex.x - 1, hex.y + 0, hex.z + 1);
        neighbours[5] = new Vector3(hex.x + 0, hex.y - 1, hex.z + 1);

        return neighbours;
    }

    public static void PIDAngleAdjust(ref float angle, ref float desiredAngle)
    {


        if (angle - desiredAngle > 180)
        {
            desiredAngle += 360f;
        }
        else if (angle - desiredAngle < -180)
            desiredAngle -= 360f;

        if (angle > 360 || desiredAngle > 360)
        {
            angle -= 360;
            desiredAngle -= 360;
        }
        else if (angle < -360 || desiredAngle < -360)
        {
            angle += 360;
            desiredAngle += 360;
        }
    }   

   public static void CopyTransform(Transform OG, ref GameObject NEWG)
    {
        if(OG.parent != null)
            NEWG.transform.parent = OG.parent;
        NEWG.transform.position = OG.position;
        NEWG.transform.localScale = OG.localScale;
        NEWG.transform.rotation = OG.rotation;
    }

}

public class PID
{
    public float pFactor, iFactor, dFactor;

    float integral;
    float lastError;


    public PID(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }


    public float Update(float setpoint, float actual, float timeFrame, float limit = 0f)
    {

        float present = setpoint - actual;
        if (present > 180)
            present -= 360;
        if (present < -180)
            present += 360;
        integral += present * timeFrame;
        float deriv = (present - lastError) / timeFrame;
        lastError = present;
        float result = present * pFactor + integral * iFactor + deriv * dFactor;
        if (limit < 0f)
            result /= 5*limit;
        if(limit > 0f && result*result > limit*limit)
        {
            if (result < 0)
                limit *= -1;
            result = limit;
        }
        return result;
    }
}