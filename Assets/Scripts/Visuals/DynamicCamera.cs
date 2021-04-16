using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{

    Camera cam;
    public const float zDist = -70f;
    public const float yDist = 21.82f;
    public const float lowerYbound = -30f;
    public const float upperYbound = 30f;

    public Vector3 target;
    public bool isFlipped;

    void Start()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDynamicCamera();
    }

    void UpdateDynamicCamera()
    {
        float AR = Screen.width / Screen.height;
        Vector3 mouse = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, 0f, 0f), 0f);
        Vector3 loc = Vector3.zero;

        if (!isFlipped)
        {
            loc = mouse + new Vector3(target.x, 0f, 0f) + Utilities.GetWorldPositionOnPlane(new Vector3(Screen.width, 0f, 0f), 0f)*1/6f;
            loc /= 2;
            if(loc.x-target.x < 0)
            {
                loc = target;
            }
            Vector3 limit = new Vector3(target.x, 0f, 0f) + Utilities.GetWorldPositionOnPlane(new Vector3(Screen.width, 0f, 0f), 0f) * 2 / 6f;
            if ((limit - target).sqrMagnitude < (loc - target).sqrMagnitude)
            {
                loc = limit;
            }
        }
        else
        {
            loc = mouse + new Vector3(target.x, 0f, 0f) - Utilities.GetWorldPositionOnPlane(new Vector3(Screen.width, 0f, 0f), 0f) * 1 / 6f;
            loc /= 2;

            if (loc.x - target.x > 0)
            {
                loc = target;
            }
            Vector3 limit = new Vector3(target.x, 0f, 0f) - Utilities.GetWorldPositionOnPlane(new Vector3(Screen.width, 0f, 0f), 0f) * 2 / 6f;
            if ((limit - target).sqrMagnitude < (loc - target).sqrMagnitude)
            {
                loc = limit;
            }
        }

        loc.y = yDist;
        loc.z = zDist;

        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, loc, 2*Time.deltaTime);

    }
}
