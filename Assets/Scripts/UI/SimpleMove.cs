using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = gameObject.transform.position;

        if (Input.GetKey("j"))
        {
            //Debug.Log("a pressed");
            pos.x -= 0.5f;
        }
        if (Input.GetKey("l"))
        {
            pos.x += 0.5f;
        }
        if (Input.GetKey("i"))
        {
            pos.y += 0.5f;
        }
        if (Input.GetKey("k"))
        {
            pos.y -= 0.5f;
        }

        gameObject.transform.position = pos;
    }
}
