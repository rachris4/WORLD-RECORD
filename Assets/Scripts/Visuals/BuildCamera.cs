using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildCamera : MonoBehaviour
{

    Camera cam;
    public const float zDist = -30f;
    public Vector3 target;
    public bool canMove;

    void Start()
    {
        cam = Utilities.FindMainCamera().GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove)
            return;

        target = Vector3.zero;
        int count = 0;
        foreach(Transform child in gameObject.transform)
        {
            target += child.position;
            count++;
        }
        if(count > 0)
            target /= count;

        UpdateDynamicCamera();
    }

    void UpdateDynamicCamera()
    {
        float AR = Screen.width / Screen.height;
        Vector3 mouse = Utilities.GetWorldPositionOnPlane(new Vector3(Input.mousePosition.x, 0f, 0f), 0f);
        Vector3 loc = new Vector3(target.x,target.y,zDist);

        cam.gameObject.transform.position = Vector3.Lerp(cam.gameObject.transform.position, loc, 3*Time.deltaTime);

    }
}
