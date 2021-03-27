using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleControl : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody2D rb;
    [SerializeField]
    private float thrust = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey("a"))
        {
            //Debug.Log("a pressed");
            rb.AddForce(new Vector2(-20f*thrust, 0f));
        }
        if (Input.GetKey("d"))
        {
            rb.AddForce(new Vector2(20f * thrust, 0f));
        }
        if (Input.GetKey("w"))
        {
            rb.AddForce(new Vector2(0f , 20f * thrust));
        }
        if (Input.GetKey("s"))
        {
            rb.AddForce(new Vector2(0f , -20f * thrust));
        }
    }
}
