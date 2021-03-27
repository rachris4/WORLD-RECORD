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